using Common.Interfaces;
using Common.Models.User;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace CoordinatorStateful
{
    internal sealed class CoordinatorStateful : StatefulService, ICoordinator
    {
        private IUsersService _userProxy = null!;
        private IProductsService _productProxy = null!;

        public CoordinatorStateful(StatefulServiceContext context) : base(context) { }

        public async Task<bool> MakePurchaseAsync(Basket basket)
        {
            if (!await _userProxy.UserExistsAsync(basket.Email)) return false;
            if (!await _productProxy.CanBuyAsync(basket.Items)) return false;

            await _productProxy.MakePurchaseAsync(basket.Items);
            await _userProxy.MakePurchaseAsync(basket);

            return true;
        }

        public async Task<bool> CanPurchaseAsync(List<Item> items)
            => await _productProxy.CanBuyAsync(items);

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
            => this.CreateServiceRemotingReplicaListeners();

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
        }
    }
}
