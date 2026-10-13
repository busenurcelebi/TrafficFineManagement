
using System;
using System.ComponentModel.DataAnnotations;

namespace TrafficFineManagement.ViewModels
{
    public class CreateTrafficFineViewModel
    {
        [Range(1, int.MaxValue,
            ErrorMessage = "Lütfen bir araç seçiniz.")]
        [Display(Name = "Araç")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Tutanak numarası zorunludur.")]
        [StringLength(50)]
        [Display(Name = "Tutanak Numarası")]
        public string FineNumber { get; set; } = string.Empty;


        [Range(typeof(decimal), "0.01", "999999999.99",
            ParseLimitsInInvariantCulture = true,
            ErrorMessage = "Geçerli bir ceza tutarı giriniz.")]
        [Display(Name = "Ceza Tutarı")]
        public decimal Amount { get; set; }


        [Required(ErrorMessage = "Ceza tarihi zorunludur.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ceza Tarihi")]
        public DateTime? FineDate { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [StringLength(500)]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;
    }
}
