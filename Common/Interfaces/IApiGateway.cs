using Common.Models.Product;
using Common.Models.User;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IApiGateway : IService
    {
        [OperationContract]
        Task<bool> LoginAsync(Login credentials);

        [OperationContract]
        Task<bool> RegisterAsync(Register credentials);

        [OperationContract]
        Task<EditProfile?> GetUserDataAsync(string email);

        [OperationContract]
        Task<bool> UpdateProfileAsync(EditProfile credentials);

        [OperationContract]
        Task<List<Product>> GetAllProductsByCategoryAsync(string category);

        [OperationContract]
        Task<bool> MakePurchaseAsync(Basket basket);
    }
}
