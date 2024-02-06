using Common.Models.Product;
using Common.Models.User;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IProductsService : IService
    {
        [OperationContract]
        Task<List<Product>> GetAllProductsByCategoryAsync(string category);

        [OperationContract]
        Task<bool> CanBuyAsync(List<Item> items);

        [OperationContract]
        Task MakePurchaseAsync(List<Item> items);
    }
}
