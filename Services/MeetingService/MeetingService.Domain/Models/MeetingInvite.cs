using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeetingService.Domain.Models
{
    public class MeetingInvite
    {
        public int Id { get; set; }
        public int MeetingId { get; set; }
        public string InviteeEmail { get; set; } = string.Empty;
        public string InvitedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public Meeting Meeting { get; set; } = null!;
    }
}
