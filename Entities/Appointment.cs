using System;

namespace ConsoleApp1.Entities
{
    public class Appointment
    {
        public string AppointmentID { get; set; }
        public string SlotID { get; set; }  // Added SlotID to link to AvailabilitySlot
        public string CustomerID { get; set; }
        public string BarberID { get; set; }
        public string ServiceID { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public string Notes { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime BookingDate { get; set; }
    }
}
