using System;

namespace PantryChef.Data.Entities
{
    public class SystemNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string EventKey { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeliveredAt { get; set; }

        public virtual User User { get; set; }
    }
}
