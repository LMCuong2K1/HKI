using System;
using System.Collections.Generic;

namespace ConsoleApp1.Entities
{
    public enum OrderStatus
    {
        PENDING,
        PAID,
        PROCESSING,
        SHIPPED,
        DELIVERED,
        CANCELLED
    }

    public class Order
    {
        public string OrderID { get; set; }
        public string CustomerID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public void CalculateTotalAmount()
        {
            decimal total = 0;
            foreach (var item in OrderItems)
            {
                total += item.CalculateSubtotal();
            }
            TotalAmount = total;
        }

        public void AddOrderItem(OrderItem item)
        {
            OrderItems.Add(item);
            CalculateTotalAmount();
        }

        public bool RemoveOrderItem(string orderItemId)
        {
            var item = OrderItems.Find(i => i.OrderItemID == orderItemId);
            if (item != null)
            {
                OrderItems.Remove(item);
                CalculateTotalAmount();
                return true;
            }
            return false;
        }
    }

    public class OrderItem
    {
        public string OrderItemID { get; set; }
        public string ProductID { get; set; }
        public string ProductNameSnapshot { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtOrder { get; set; }

        public decimal CalculateSubtotal()
        {
            return Quantity * PriceAtOrder;
        }
    }
}
