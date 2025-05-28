using System;
using System.Collections.Generic;
using ConsoleApp1.Entities;
using MySql.Data.MySqlClient;

namespace ConsoleApp1.Services
{
    public class InventoryService
    {
        private readonly string _connectionString;

        public InventoryService()
        {
            _connectionString = DataAccess.DatabaseConfig.ConnectionString;
        }

        // Product methods
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Products";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new Product
                            {
                                ProductID = reader["ProductID"].ToString(),
                                ProductName = reader["ProductName"].ToString(),
                                Description = reader["Description"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                QuantityInStock = Convert.ToInt32(reader["QuantityInStock"]),
                                Category = reader["Category"].ToString(),
                                ImageURL = reader["ImageURL"].ToString(),
                                DateAdded = Convert.ToDateTime(reader["DateAdded"])
                            };
                            products.Add(product);
                        }
                    }
                }
            }
            return products;
        }

        public bool AddNewProduct(Product product)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string insertQuery = @"INSERT INTO Products (ProductID, ProductName, Description, Price, QuantityInStock, Category, ImageURL, DateAdded)
                                       VALUES (@ProductID, @ProductName, @Description, @Price, @QuantityInStock, @Category, @ImageURL, @DateAdded)";
                using (var cmd = new MySqlCommand(insertQuery, connection))
                {
                    product.ProductID = Guid.NewGuid().ToString();
                    product.DateAdded = DateTime.Now;

                    cmd.Parameters.AddWithValue("@ProductID", product.ProductID);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@Price", product.Price);
                    cmd.Parameters.AddWithValue("@QuantityInStock", product.QuantityInStock);
                    cmd.Parameters.AddWithValue("@Category", product.Category);
                    cmd.Parameters.AddWithValue("@ImageURL", product.ImageURL);
                    cmd.Parameters.AddWithValue("@DateAdded", product.DateAdded);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool UpdateExistingProduct(Product product)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string updateQuery = @"UPDATE Products SET ProductName = @ProductName, Description = @Description, Price = @Price, 
                                       QuantityInStock = @QuantityInStock, Category = @Category, ImageURL = @ImageURL WHERE ProductID = @ProductID";
                using (var cmd = new MySqlCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", product.ProductID);
                    cmd.Parameters.AddWithValue("@ProductName", product.ProductName);
                    cmd.Parameters.AddWithValue("@Description", product.Description);
                    cmd.Parameters.AddWithValue("@Price", product.Price);
                    cmd.Parameters.AddWithValue("@QuantityInStock", product.QuantityInStock);
                    cmd.Parameters.AddWithValue("@Category", product.Category);
                    cmd.Parameters.AddWithValue("@ImageURL", product.ImageURL);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool DeleteExistingProduct(string productId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Products WHERE ProductID = @ProductID";
                using (var cmd = new MySqlCommand(deleteQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        // Service methods
        public List<Service> GetAllServices()
        {
            var services = new List<Service>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Services";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var service = new Service
                            {
                                ServiceID = reader["ServiceID"].ToString(),
                                ServiceName = reader["ServiceName"].ToString(),
                                Description = reader["Description"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Duration = TimeSpan.FromMinutes(Convert.ToInt32(reader["DurationMinutes"])),
                                Category = reader["Category"].ToString()
                            };
                            services.Add(service);
                        }
                    }
                }
            }
            return services;
        }

        public bool AddNewService(Service service)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string insertQuery = @"INSERT INTO Services (ServiceID, ServiceName, Description, Price, DurationMinutes, Category)
                                       VALUES (@ServiceID, @ServiceName, @Description, @Price, @DurationMinutes, @Category)";
                using (var cmd = new MySqlCommand(insertQuery, connection))
                {
                    service.ServiceID = Guid.NewGuid().ToString();

                    cmd.Parameters.AddWithValue("@ServiceID", service.ServiceID);
                    cmd.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                    cmd.Parameters.AddWithValue("@Description", service.Description);
                    cmd.Parameters.AddWithValue("@Price", service.Price);
                    cmd.Parameters.AddWithValue("@DurationMinutes", service.Duration.TotalMinutes);
                    cmd.Parameters.AddWithValue("@Category", service.Category);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool UpdateExistingService(Service service)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string updateQuery = @"UPDATE Services SET ServiceName = @ServiceName, Description = @Description, Price = @Price, 
                                       DurationMinutes = @DurationMinutes, Category = @Category WHERE ServiceID = @ServiceID";
                using (var cmd = new MySqlCommand(updateQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ServiceID", service.ServiceID);
                    cmd.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                    cmd.Parameters.AddWithValue("@Description", service.Description);
                    cmd.Parameters.AddWithValue("@Price", service.Price);
                    cmd.Parameters.AddWithValue("@DurationMinutes", service.Duration.TotalMinutes);
                    cmd.Parameters.AddWithValue("@Category", service.Category);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }

        public bool DeleteExistingService(string serviceId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string deleteQuery = "DELETE FROM Services WHERE ServiceID = @ServiceID";
                using (var cmd = new MySqlCommand(deleteQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@ServiceID", serviceId);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
    }
}
