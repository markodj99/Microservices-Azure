using System.Runtime.Serialization;

namespace Common.Models.Product
{
    [DataContract]
    public class Product
    {
        [DataMember]
        public string Category { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Desc { get; set; }

        [DataMember]
        public int Price { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        public Product() { }

        public Product(ProductsTable product)
        {
            Category = product.Category;
            Name = product.Name;
            Desc = product.Desc;
            Price = product.Price;
            Quantity = product.Quantity;
        }
    }
}
