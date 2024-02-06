using Azure.Data.Tables;
using Common.Interfaces;
using Common.Models.Product;
using Common.Models.User;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Diagnostics;
using System.Fabric;

namespace ProductsStateful
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ProductsStateful : StatefulService, IProductsService
    {
        private TableClient productTable = null;
        private Thread productTableThread = null;
        private IReliableDictionary<string, Product> productDictionary = null;

        public ProductsStateful(StatefulServiceContext context) : base(context) { }

        public async Task<List<Product>> GetAllProductsByCategoryAsync(string category)
        {
            var products = new List<Product>();

            using (var tx = StateManager.CreateTransaction())
            {
                var enumerator = (await productDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    if (!enumerator.Current.Value.Category.Equals(category)) continue;
                    products.Add(enumerator.Current.Value);
                }
            }

            return products;
        }

        public async Task<bool> CanBuyAsync(List<Item> items)
        {
            foreach (var item in items)
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var productResult = await productDictionary.TryGetValueAsync(tx, item.Name);

                    if (!productResult.HasValue) return false;
                    if (productResult.Value.Quantity - item.Quantity < 0) return false;
                }
            }

            return true;
        }

        public async Task MakePurchaseAsync(List<Item> items)
        {
            foreach (var item in items)
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var productResult = await productDictionary.TryGetValueAsync(tx, item.Name);

                    if (productResult.HasValue)
                    {
                        var oldProduct = productResult.Value;
                        var newProduct = new Product { 
                            Name = oldProduct.Name,
                            Category = oldProduct.Category,
                            Desc = oldProduct.Desc,
                            Price = oldProduct.Price,
                            Quantity = oldProduct.Quantity - item.Quantity
                        };

                        await productDictionary.TryUpdateAsync(tx, oldProduct.Name, newProduct, oldProduct);
                        await tx.CommitAsync();
                    }
                }
            }
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
            => this.CreateServiceRemotingReplicaListeners();

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await SetTableAsync();
            productDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, Product>>("Product");
            await PopulateProductDictionary();

            productTableThread = new Thread(new ThreadStart(ProductTableWriteThread));
            productTableThread.Start();
        }

        private async Task SetTableAsync()
        {
            var tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");
            await tableServiceClient.CreateTableIfNotExistsAsync("Product");
            productTable = tableServiceClient.GetTableClient("Product");
        }

        private async Task PopulateProductDictionary()
        {
            var entities = productTable.QueryAsync<ProductsTable>(x => true).GetAsyncEnumerator();

            using (var tx = StateManager.CreateTransaction())
            {
                while (await entities.MoveNextAsync())
                {
                    var product = new Product(entities.Current);
                    await productDictionary.TryAddAsync(tx, product.Name, product);
                }

                await tx.CommitAsync();
            }
        }

        private async void ProductTableWriteThread()
        {
            while (true)
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var enumerator = (await productDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var product = enumerator.Current.Value;
                        await productTable.UpsertEntityAsync(new ProductsTable(product), TableUpdateMode.Merge, CancellationToken.None);
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
