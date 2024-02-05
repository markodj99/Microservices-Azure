using System.Fabric;
using Common.Interfaces;
using Common.Models.User;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure;

namespace UsersStateful
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class UsersStateful : StatefulService, IUsersService
    {
        private TableClient userTable;
        private Thread userTableThread;
        private IReliableDictionary<string, User> userDictionary;

        public UsersStateful(StatefulServiceContext context) : base(context) { }

        public async Task<bool> LoginAsync(Login credentials)
        {
            return false;
        }

        public async Task<bool> RegisterAsync(Register credentials)
        {
            return false;
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
            userDictionary = await StateManager.GetOrAddAsync<IReliableDictionary<string, User>>("User");

            userTableThread = new Thread(new ThreadStart(UserTableWriteThread));
            userTableThread.Start();
        }

        private async Task SetTableAsync()
        {
            string connectionString = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
            var tableServiceClient = new TableServiceClient(connectionString);
            await tableServiceClient.CreateTableIfNotExistsAsync("User");
            userTable = tableServiceClient.GetTableClient("User");

            //using (var tx = StateManager.CreateTransaction())
            //{
            //    var user = new User()
            //    {
            //        Email = "test@gmail.com",
            //        Username = "test",
            //        Password = "test"
            //    };

            //    await userTable.AddEntityAsync(new UsersTable(user), CancellationToken.None);
            //}
        }

        private async void UserTableWriteThread()
        {
            while (true)
            {
                using (var tx = StateManager.CreateTransaction())
                {
                    var enumerator = (await userDictionary.CreateEnumerableAsync(tx)).GetAsyncEnumerator();

                    while (await enumerator.MoveNextAsync(CancellationToken.None))
                    {
                        var user = enumerator.Current.Value;
                        await userTable.UpdateEntityAsync(new UsersTable(user), ETag.All, TableUpdateMode.Merge, CancellationToken.None);
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
