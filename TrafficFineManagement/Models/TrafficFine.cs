
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TrafficFineManagement.Enums;

namespace TrafficFineManagement.Models
{
    public class TrafficFine
    {
        public int Id { get; set; }

        [Required]
        public int VehicleId { get; set; }

        [Required]
        [StringLength(50)]
        public string FineNumber { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999.99")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FineDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public FineStatus Status { get; set; }
            = FineStatus.New;

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public Vehicle Vehicle { get; set; } = null!;


        public ICollection<ApprovalHistory> ApprovalHistories { get; set; }
            = new List<ApprovalHistory>();

    }
}
