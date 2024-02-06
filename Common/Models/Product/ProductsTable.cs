using Azure;
using Azure.Data.Tables;

namespace Common.Models.Product
{
    public class ProductsTable : ITableEntity
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }

        public string PartitionKey { get; set; } = null!;
        public string RowKey { get; set; } = null!;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public ProductsTable(Product product)
        {
            Category = product.Category;
            Name = product.Name;
            Desc = product.Desc;
            Price = product.Price;
            Quantity = product.Quantity;

            PartitionKey = "Product";
            RowKey = product.Name;
            Timestamp = DateTimeOffset.Now;
            ETag = ETag.All;
        }

        public ProductsTable() { }
    }
}
