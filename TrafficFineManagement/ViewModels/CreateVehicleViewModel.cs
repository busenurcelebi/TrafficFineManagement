
using System.ComponentModel.DataAnnotations;
using TrafficFineManagement.Enums;

namespace TrafficFineManagement.ViewModels
{
    public class CreateVehicleViewModel
    {
        [Required(ErrorMessage = "Plaka zorunludur.")]
        [StringLength(15)]
        [Display(Name = "Plaka")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Araç tipi seçiniz.")]
        [EnumDataType(typeof(VehicleType),
            ErrorMessage = "Geçerli bir araç tipi seçiniz.")]
        [Display(Name = "Araç Tipi")]
        public VehicleType? VehicleType { get; set; }

        [Required(ErrorMessage = "Marka zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Marka")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Model")]
        public string Model { get; set; } = string.Empty;
    }
}
