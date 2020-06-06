using System;

namespace TestSingalR.Entity
{
    public class Activity
    {
        public Guid Id { get; set; }
        public string AppUserId { get; set; }
        public virtual AppUser AppUser { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
    }
}
