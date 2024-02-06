using Common.Models.User;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface IUsersService : IService
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
        Task<bool> UserExistsAsync(string email);

        [OperationContract]
        Task MakePurchaseAsync(Basket basket);
    }
}
