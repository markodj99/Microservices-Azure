using System.Runtime.Serialization;

namespace Common.Models.User
{
    [DataContract]
    public class Item
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        public Item () { }

        public Item (string name) 
        { 
            Name = name;
            Quantity = 1;
        }
    }
}
