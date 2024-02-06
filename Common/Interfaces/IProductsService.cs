using Common.Models.Product;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IProductsService : IService
    {
        [OperationContract]
        Task<List<Product>> GetAllProductsByCategory(string category);
    }
}
