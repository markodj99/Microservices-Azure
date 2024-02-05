using Azure;
using Azure.Data.Tables;

namespace Common.Models.User
{
    public class UsersTable : ITableEntity
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public UsersTable(Register user)
        {
            PartitionKey = "User";
            RowKey = user.Email;
            Timestamp = DateTimeOffset.Now;
            ETag = ETag.All;

            Username = user.Username;
            Email = user.Email;
            Password = user.Password;
        }

        public UsersTable(User user)
        {
            PartitionKey = "User";
            RowKey = user.Email;
            Timestamp = DateTimeOffset.Now;
            ETag = ETag.All;

            Username = user.Username;
            Email = user.Email;
            Password = user.Password;
        }

        public UsersTable() { }
    }
}
