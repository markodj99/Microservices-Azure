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


        public Basket() { }

        public Basket(string email)
        {
            Email = email;
            Items = new List<Item>();
        }

        public void AddItem(string name)
        {
            foreach (var item in Items)
            {
                if (item.Name.Equals(name))
                {
                    item.Quantity++;
                    return;
                }
            }

            Items.Add(new Item(name));
        }

        public int GetCount()
        {
            int count = 0;
            foreach (var item in Items) count += item.Quantity;
            return count;
        }
    }
}
