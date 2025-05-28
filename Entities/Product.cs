using System;

namespace ConsoleApp1.Entities
{
    public class Product
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int QuantityInStock { get; set; }
        public string Category { get; set; }
        public string ImageURL { get; set; }
        public DateTime DateAdded { get; set; }

        public bool DecreaseStock(int quantity)
        {
            if (QuantityInStock >= quantity)
            {
                QuantityInStock -= quantity;
                return true;
            }
            return false;
        }

        public void IncreaseStock(int quantity)
        {
            QuantityInStock += quantity;
        }
    }
}
