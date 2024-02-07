using Common.Interfaces;
using Common.Models.Product;
using Common.Models.User;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace ApiGatewayStateless
{
    internal sealed class ApiGatewayStateless : StatelessService, IApiGateway
    {
        private IUsersService _userProxy = null!;
        private IProductsService _productProxy = null!;
        private ICoordinator _coordinatorProxy = null!;

        public ApiGatewayStateless(StatelessServiceContext context) : base(context) { }

        public async Task<bool> LoginAsync(Login credentials)
            => await _userProxy.LoginAsync(credentials);

        public async Task<bool> RegisterAsync(Register credentials)
            => await _userProxy.RegisterAsync(credentials);

        public async Task<EditProfile?> GetUserDataAsync(string email)
            => await _userProxy.GetUserDataAsync(email);

        public async Task<bool> UpdateProfileAsync(EditProfile credentials)
            => await _userProxy.UpdateProfileAsync(credentials);

        public async Task<List<Product>> GetAllProductsByCategoryAsync(string category)
            => await _productProxy.GetAllProductsByCategoryAsync(category);

        public async Task<bool> MakePurchaseAsync(Basket basket) 
            => await _coordinatorProxy.MakePurchaseAsync(basket);

        public async Task<bool> CanPurchaseAsync(List<Item> items)
            => await _coordinatorProxy.CanPurchaseAsync(items);

        public async Task<List<UserPurchase>> GetHistoryAsync(string email)
            => await _userProxy.GetHistoryAsync(email);

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
            => this.CreateServiceRemotingInstanceListeners();

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var fabricClient = new FabricClient();
            var serviceUri = new Uri("fabric:/Cloud-Project/UsersStateful");
            var partitionList = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

            foreach (var partition in partitionList)
            {
                var partitionKey = partition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    _userProxy = ServiceProxy.Create<IUsersService>(serviceUri, servicePartitionKey);
                    break;
                }
            }

            fabricClient = new FabricClient();
            serviceUri = new Uri("fabric:/Cloud-Project/ProductsStateful");
            partitionList = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

            foreach (var partition in partitionList)
            {
                var partitionKey = partition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    _productProxy = ServiceProxy.Create<IProductsService>(serviceUri, servicePartitionKey);
                    break;
                }
            }

            fabricClient = new FabricClient();
            serviceUri = new Uri("fabric:/Cloud-Project/CoordinatorStateful");
            partitionList = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

            foreach (var partition in partitionList)
            {
                var partitionKey = partition.PartitionInformation as Int64RangePartitionInformation;

                if (partitionKey != null)
                {
                    var servicePartitionKey = new ServicePartitionKey(partitionKey.LowKey);

                    _coordinatorProxy = ServiceProxy.Create<ICoordinator>(serviceUri, servicePartitionKey);
                    break;
                }
            }
        }
    }
}
