using Common.Models.User;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Common.Interfaces
{
    [ServiceContract]
    public interface ICoordinator : IService
    {
        [OperationContract]
        Task<bool> MakePurchaseAsync(Basket basket);
    }
}
