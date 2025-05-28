using System;

namespace ConsoleApp1.Entities
{
    public class AvailabilitySlot
    {
        public string SlotID { get; set; }
        public string BarberID { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; } = true;

        public bool IsCurrentlyAvailable()
        {
            return IsAvailable;
        }

        public void BookSlot()
        {
            IsAvailable = false;
        }

        public void ReleaseSlot()
        {
            IsAvailable = true;
        }
    }
}
