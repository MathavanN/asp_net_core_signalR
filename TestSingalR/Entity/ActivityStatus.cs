using System;

namespace TestSingalR.Entity
{
    public class ActivityStatus
    {
        public Guid Id { get; set; }

        public string Status { get; set; }

        public DateTime CompletedAt { get; set; }
    }
}
