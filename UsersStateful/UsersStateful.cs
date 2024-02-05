using Azure.Data.Tables;
using Common.Interfaces;
using Common.Models.User;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.Fabric;

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
            bool status = false;

            using (var tx = StateManager.CreateTransaction())
            {
                var userResult = await userDictionary.TryGetValueAsync(tx, credentials.Email);

                if (userResult.HasValue)
                {
                    if (userResult.Value.Password == credentials.Password) status = true;
                }
            }

            return status;
        }

        public async Task<bool> RegisterAsync(Register credentials)
        {
            bool status = false;

            using (var tx = StateManager.CreateTransaction())
            {
                var userResult = await userDictionary.TryGetValueAsync(tx, credentials.Email);

                if (!userResult.HasValue)
                {
                    if (!credentials.Password.Equals(credentials.ConfirmPassword)) status = false;
                    else
                    {
                        try
                        {
                            await userDictionary.AddAsync(tx, credentials.Email, new User(credentials));
                            await tx.CommitAsync();
                            status = true;
                        }
                        catch (Exception)
                        {
                            status = false;
                            tx.Abort();
                        }
                    }
                }
            }

            return status;
        }

        public async Task<EditProfile?> GetUserDataAsync(string email)
        {
            using (var tx = StateManager.CreateTransaction())
            {
                var userResult = await userDictionary.TryGetValueAsync(tx, email);

                if (userResult.HasValue) return new EditProfile(userResult.Value);
                return null;
            }
        }

        public async Task<bool> UpdateProfileAsync(EditProfile credentials)
        {
            bool status = false;

            using (var tx = StateManager.CreateTransaction())
            {
                var userResult = await userDictionary.TryGetValueAsync(tx, credentials.Email);

                if (!userResult.HasValue) status = false;
                else
                {
                    var user = userResult.Value;

                    if (!credentials.ConfirmOldPassword.Equals(user.Password)) status = false;
                    else if(!credentials.NewPassword.Equals(credentials.ConfirmNewPassword)) status = false;
                    else
                    {
                        try
                        {
                            await userDictionary.TryUpdateAsync(tx, user.Email, new User(credentials), user);
                            await tx.CommitAsync();
                            status = true;
                        }
                        catch (Exception)
                        {
                            status = false;
                            tx.Abort();
                        }
                    }
                }
            }

            return status;
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
            await PopulateUserDictionary();

            userTableThread = new Thread(new ThreadStart(UserTableWriteThread));
            userTableThread.Start();
        }

        private async Task SetTableAsync()
        {
            var tableServiceClient = new TableServiceClient("UseDevelopmentStorage=true");
            await tableServiceClient.CreateTableIfNotExistsAsync("User");
            userTable = tableServiceClient.GetTableClient("User");
        }

        private async Task PopulateUserDictionary()
        {
            var entities = userTable.QueryAsync<UsersTable>(x => true).GetAsyncEnumerator();

            using (var tx = StateManager.CreateTransaction())
            {

                while (await entities.MoveNextAsync())
                {
                    var user = new User(entities.Current);
                    await userDictionary.TryAddAsync(tx, user.Email, user);
                }

                await tx.CommitAsync();
            }
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
                        await userTable.UpsertEntityAsync(new UsersTable(user), TableUpdateMode.Merge, CancellationToken.None);
                    }
                }

                Thread.Sleep(5000);
            }
        }
    }
}
