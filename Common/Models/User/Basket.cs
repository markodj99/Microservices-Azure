using System.Runtime.Serialization;

namespace Common.Models.User
{
    [DataContract]
    public class Basket
    {
        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public List<Item> Items { get; set; }

        [DataMember]
        public string PaymentMethod { get; set; }

        public Basket() { }

        public Basket(string email)
        {
            Email = email;
            Items = new List<Item>();
            PaymentMethod = "Unknown";
        }

        public void AddItem(string name, int price)
        {
            foreach (var item in Items)
            {
                if (item.Name.Equals(name))
                {
                    item.Quantity++;
                    return;
                }
            }

            Items.Add(new Item(name, price));
        }

        public void RemoveOne(string name)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Name.Equals(name))
                {
                    if (Items[i].Quantity - 1 < 1) Items.RemoveAt(i);
                    else Items[i].Quantity--;

                    return;
                }
            }
        }
    }
}
