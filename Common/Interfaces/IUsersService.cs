﻿using Common.Models.User;
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
    }
}