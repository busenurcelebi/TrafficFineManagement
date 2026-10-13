
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrafficFineManagement.Data;
using TrafficFineManagement.Enums;
using TrafficFineManagement.Models;
using TrafficFineManagement.ViewModels;
using System.Security.Claims;
using TrafficFineManagement.Services;
using TrafficFineManagement.Seed;

namespace TrafficFineManagement.Controllers
{
    [Authorize]
    public class TrafficFinesController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IApprovalService _approvalService;


        public TrafficFinesController(
            ApplicationDbContext context,
            IApprovalService approvalService)
        {
            _context = context;
            _approvalService = approvalService;
        }


        // GET: TrafficFines
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var fines = await _context.TrafficFines
                .AsNoTracking()
                .Include(f => f.Vehicle)
                .OrderByDescending(f => f.CreatedAt)
                .ThenByDescending(f => f.Id)
                .ToListAsync();

            return View(fines);
        }

        // GET: TrafficFines/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadVehiclesAsync();

            return View(new CreateTrafficFineViewModel());
        }

        // POST: TrafficFines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreateTrafficFineViewModel input)
        {
            input.FineNumber =
                input.FineNumber?.Trim().ToUpperInvariant()
                ?? string.Empty;

            if (input.FineDate.HasValue &&
                input.FineDate.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(input.FineDate),
                    "Ceza tarihi gelecekte olamaz.");
            }

            if (!ModelState.IsValid)
            {
                await LoadVehiclesAsync();
                return View(input);
            }

            bool vehicleExists = await _context.Vehicles
                .AnyAsync(v => v.Id == input.VehicleId);

            if (!vehicleExists)
            {
                ModelState.AddModelError(
                    nameof(input.VehicleId),
                    "Seçilen araç bulunamadı.");

                await LoadVehiclesAsync();
                return View(input);
            }

            bool fineExists = await _context.TrafficFines
                .AnyAsync(f => f.FineNumber == input.FineNumber);

            if (fineExists)
            {
                ModelState.AddModelError(
                    nameof(input.FineNumber),
                    "Bu tutanak numarası zaten kayıtlı.");

                await LoadVehiclesAsync();
                return View(input);
            }

            var fine = new TrafficFine
            {
                VehicleId = input.VehicleId,
                FineNumber = input.FineNumber,
                Amount = input.Amount,
                FineDate = input.FineDate!.Value,
                Description = input.Description.Trim(),
                Status = FineStatus.PendingManagerApproval,
                CreatedAt = DateTime.UtcNow
            };

            _context.TrafficFines.Add(fine);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Ceza kaydedilemedi. Tutanak numarası " +
                    "tekrar ediyor olabilir veya " +
                    "veritabanı işlemi başarısız oldu.");

                await LoadVehiclesAsync();
                return View(input);
            }

            TempData["SuccessMessage"] =
                "Trafik cezası başarıyla kaydedildi.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadVehiclesAsync()
        {
            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .OrderBy(v => v.PlateNumber)
                .Select(v => new
                {
                    v.Id,
                    v.PlateNumber
                })
                .ToListAsync();

            ViewBag.Vehicles = new SelectList(
                vehicles,
                "Id",
                "PlateNumber");
        }


        // GET: TrafficFines/Details/1
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var fine = await _context.TrafficFines
                .AsNoTracking()
                .Include(f => f.Vehicle)
                .Include(f => f.ApprovalHistories)
                    .ThenInclude(h => h.User)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fine == null)
            {
                return NotFound();
            }

            return View(fine);
        }


        // POST: TrafficFines/ProcessApproval

        [HttpPost]
        [Authorize(Roles = "Manager,Finance")]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> ProcessApproval(
            int trafficFineId,
            ApprovalAction action,
            string? explanation)
        {
            string? userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            bool isManager = User.IsInRole(
                IdentitySeeder.ManagerRole);

            bool isFinance = User.IsInRole(
                IdentitySeeder.FinanceRole);

            var result = await _approvalService.ProcessAsync(
                trafficFineId,
                userId,
                isManager,
                isFinance,
                action,
                explanation);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(
                nameof(Details),
                new { id = trafficFineId });
        }


    }
}
