using Azure;
using Azure.Data.Tables;

namespace Common.Models.User
{
    public class UserPurchasesTable : ITableEntity
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Method { get; set; }
        public int Amount { get; set; }
        public DateTime Time { get; set; }
        public int Items { get; set; }

        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public UserPurchasesTable(UserPurchase table)
        {
            PartitionKey = "UserPurchase";
            RowKey = table.Id;
            Timestamp = DateTimeOffset.Now;
            ETag = ETag.All;

            Id = table.Id;
            Email = table.Email;
            Method = table.Method;
            Time = table.Time;
            Items = table.Items;
            Amount = table.Amount;
        }

        public UserPurchasesTable() { }
    }
}
