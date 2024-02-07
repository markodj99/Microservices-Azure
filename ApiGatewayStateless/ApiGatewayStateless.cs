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
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ApiGatewayStateless : StatelessService, IApiGateway
    {
        private readonly IUsersService _userProxy
            = ServiceProxy.Create<IUsersService>(new Uri("fabric:/Cloud-Project/UsersStateful"), new ServicePartitionKey(1));

        private readonly IProductsService _productProxy
            = ServiceProxy.Create<IProductsService>(new Uri("fabric:/Cloud-Project/ProductsStateful"), new ServicePartitionKey(1));

        private readonly ICoordinator _coordinatorProxy
            = ServiceProxy.Create<ICoordinator>(new Uri("fabric:/Cloud-Project/CoordinatorStateful"), new ServicePartitionKey(1));

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

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
