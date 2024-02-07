using Common.Interfaces;
using Common.Models.User;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

namespace CoordinatorStateful
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class CoordinatorStateful : StatefulService, ICoordinator
    {
        private readonly IUsersService _userProxy
            = ServiceProxy.Create<IUsersService>(new Uri("fabric:/Cloud-Project/UsersStateful"), new ServicePartitionKey(1));

        private readonly IProductsService _productProxy
            = ServiceProxy.Create<IProductsService>(new Uri("fabric:/Cloud-Project/ProductsStateful"), new ServicePartitionKey(1));

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

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            var myDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, long>>("myDictionary");

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var result = await myDictionary.TryGetValueAsync(tx, "Counter");

                    ServiceEventSource.Current.ServiceMessage(this.Context, "Current Counter Value: {0}",
                        result.HasValue ? result.Value.ToString() : "Value does not exist.");

                    await myDictionary.AddOrUpdateAsync(tx, "Counter", 0, (key, value) => ++value);

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
