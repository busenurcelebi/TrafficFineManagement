
using System;
using System.ComponentModel.DataAnnotations;
using TrafficFineManagement.Enums;

namespace TrafficFineManagement.Models
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Plaka zorunludur.")]
        [StringLength(15)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Araç tipi zorunludur.")]
        public VehicleType VehicleType { get; set; }

        [Required(ErrorMessage = "Marka zorunludur.")]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model zorunludur.")]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<TrafficFine> TrafficFines { get; set; }
            = new List<TrafficFine>();

    }

}
