
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficFineManagement.Data;
using TrafficFineManagement.Models;
using TrafficFineManagement.ViewModels;

namespace TrafficFineManagement.Controllers
{
    [Authorize]
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Vehicles
        public async Task<IActionResult> Index()
        {
            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .OrderByDescending(v => v.CreatedAt)
                .ThenByDescending(v => v.Id)
                .ToListAsync();

            return View(vehicles);
        }

        // GET: Vehicles/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateVehicleViewModel());
        }

        // POST: Vehicles/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateVehicleViewModel input)
        {
            if (!string.IsNullOrWhiteSpace(input.PlateNumber))
            {
                input.PlateNumber = string.Concat(
                    input.PlateNumber.Where(c => !char.IsWhiteSpace(c)))
                    .ToUpperInvariant();
            }

            ModelState.Remove(nameof(input.PlateNumber));

            if (!TryValidateModel(input))
            {
                return View(input);
            }

            bool plateExists = await _context.Vehicles
                .AnyAsync(v => v.PlateNumber == input.PlateNumber);

            if (plateExists)
            {
                ModelState.AddModelError(
                    nameof(input.PlateNumber),
                    "Bu plakaya sahip bir araç zaten kayıtlı.");

                return View(input);
            }

            var vehicle = new Vehicle
            {
                PlateNumber = input.PlateNumber ?? string.Empty,
                VehicleType = input.VehicleType!.Value,
                Brand = input.Brand?.Trim() ?? string.Empty,
                Model = input.Model?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Vehicles.Add(vehicle);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Araç kaydedilemedi. Plaka daha önce " +
                    "kullanılmış olabilir veya veritabanı " +
                    "işlemi başarısız olmuş olabilir.");

                return View(input);
            }

            TempData["SuccessMessage"] =
                "Araç başarıyla kaydedildi.";

            return RedirectToAction(nameof(Index));
        }

    }
}
