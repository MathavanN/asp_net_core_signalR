using System;

namespace TestSingalR.Dto
{
    public class ActivityDto
    {
        public Guid Id { get; set; }
        public string AppUserId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string Result { get; set; }
    }
}
