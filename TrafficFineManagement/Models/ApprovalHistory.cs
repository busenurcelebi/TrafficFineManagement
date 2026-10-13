
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using TrafficFineManagement.Enums;

namespace TrafficFineManagement.Models
{
    public class ApprovalHistory
    {
        public int Id { get; set; }

        public int TrafficFineId { get; set; }

        public TrafficFine TrafficFine { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser User { get; set; } = null!;

        public DateTime ActionDate { get; set; } = DateTime.UtcNow;

        public ApprovalAction Action { get; set; }

        [StringLength(500)]
        public string? Explanation { get; set; }

        public FineStatus PreviousStatus { get; set; }

        public FineStatus NewStatus { get; set; }
    }
}
