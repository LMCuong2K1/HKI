using System;

namespace ConsoleApp1.Entities
{
    public class Service
    {
        public string ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
        public string Category { get; set; }
    }
}
