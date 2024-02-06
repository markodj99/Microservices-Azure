using Azure;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace Common.Models.User
{
    [DataContract]
    public class UserPurchase
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Method { get; set; }

        [DataMember]
        public int Amount { get; set; }

        [DataMember]
        public DateTime Time { get; set; }

        [DataMember]
        public int Items { get; set; }

        public UserPurchase() { }

        public UserPurchase(UserPurchasesTable table)
        {
            Id = table.Id;
            Email = table.Email;
            Method = table.Method;
            Amount = table.Amount;
            Time = table.Time;
            Items = table.Items;
        }

        public UserPurchase(Basket basket)
        {
            Id = Guid.NewGuid().ToString();
            Time = DateTime.UtcNow;
            Email = basket.Email;
            Method = basket.PaymentMethod;

            int items = 0;
            int amount = 0;

            foreach (var item in basket.Items)
            {
                items += item.Quantity;
                amount += item.Price * item.Quantity;
            }

            Items = items;
            Amount = amount;
        }
    }
}
