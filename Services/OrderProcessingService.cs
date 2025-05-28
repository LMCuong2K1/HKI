using System;
using System.Collections.Generic;
using ConsoleApp1.Entities;
using MySql.Data.MySqlClient;

namespace ConsoleApp1.Services
{
    public class OrderProcessingService
    {
        private readonly string _connectionString;

        public OrderProcessingService()
        {
            _connectionString = DataAccess.DatabaseConfig.ConnectionString;
        }

        public bool CreateOrder(Order order)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string insertOrderQuery = @"INSERT INTO Orders (OrderID, CustomerID, OrderDate, TotalAmount, Status, ShippingAddress, PaymentMethod, Notes)
                                            VALUES (@OrderID, @CustomerID, @OrderDate, @TotalAmount, @Status, @ShippingAddress, @PaymentMethod, @Notes)";
                using (var cmd = new MySqlCommand(insertOrderQuery, connection))
                {
                    order.OrderID = Guid.NewGuid().ToString();
                    order.OrderDate = DateTime.Now;
                    order.Status = OrderStatus.PENDING;

                    cmd.Parameters.AddWithValue("@OrderID", order.OrderID);
                    cmd.Parameters.AddWithValue("@CustomerID", order.CustomerID);
                    cmd.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                    cmd.Parameters.AddWithValue("@TotalAmount", order.TotalAmount);
                    cmd.Parameters.AddWithValue("@Status", order.Status.ToString());
                    cmd.Parameters.AddWithValue("@ShippingAddress", order.ShippingAddress);
                    cmd.Parameters.AddWithValue("@PaymentMethod", order.PaymentMethod);
                    cmd.Parameters.AddWithValue("@Notes", order.Notes);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                        return false;
                }

                foreach (var item in order.OrderItems)
                {
                    string insertItemQuery = @"INSERT INTO OrderItems (OrderItemID, OrderID, ProductID, ProductNameSnapshot, Quantity, PriceAtOrder)
                                               VALUES (@OrderItemID, @OrderID, @ProductID, @ProductNameSnapshot, @Quantity, @PriceAtOrder)";
                    using (var cmd = new MySqlCommand(insertItemQuery, connection))
                    {
                        item.OrderItemID = Guid.NewGuid().ToString();

                        cmd.Parameters.AddWithValue("@OrderItemID", item.OrderItemID);
                        cmd.Parameters.AddWithValue("@OrderID", order.OrderID);
                        cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                        cmd.Parameters.AddWithValue("@ProductNameSnapshot", item.ProductNameSnapshot);
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@PriceAtOrder", item.PriceAtOrder);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                            return false;
                    }
                }

                return true;
            }
        }

        public bool UpdateOrderStatus(string orderId, OrderStatus newStatus)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string updateQuery = "UPDATE Orders SET Status = @Status WHERE OrderID = @OrderID";
                using (var cmd = new MySqlCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", newStatus.ToString());
                    cmd.Parameters.AddWithValue("@OrderID", orderId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public List<Order> GetOrdersByCustomer(string customerId)
        {
            var orders = new List<Order>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Orders WHERE CustomerID = @CustomerID";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new Order
                            {
                                OrderID = reader["OrderID"].ToString(),
                                CustomerID = reader["CustomerID"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                Status = Enum.TryParse(reader["Status"].ToString(), out OrderStatus status) ? status : OrderStatus.PENDING,
                                ShippingAddress = reader["ShippingAddress"].ToString(),
                                PaymentMethod = reader["PaymentMethod"].ToString(),
                                Notes = reader["Notes"].ToString()
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }

        public List<Order> GetAllOrders()
        {
            var orders = new List<Order>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Orders";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var order = new Order
                            {
                                OrderID = reader["OrderID"].ToString(),
                                CustomerID = reader["CustomerID"].ToString(),
                                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                                TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                Status = Enum.TryParse(reader["Status"].ToString(), out OrderStatus status) ? status : OrderStatus.PENDING,
                                ShippingAddress = reader["ShippingAddress"].ToString(),
                                PaymentMethod = reader["PaymentMethod"].ToString(),
                                Notes = reader["Notes"].ToString()
                            };
                            orders.Add(order);
                        }
                    }
                }
            }
            return orders;
        }
    }
}
