
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrafficFineManagement.Data;
using TrafficFineManagement.Enums;
using TrafficFineManagement.Models;
using TrafficFineManagement.ViewModels;

namespace TrafficFineManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                TotalVehicles = await _context.Vehicles.CountAsync(),

                TotalFines = await _context.TrafficFines.CountAsync(),

                PendingManager = await _context.TrafficFines
                    .CountAsync(f =>
                        f.Status == FineStatus.PendingManagerApproval),

                PendingFinance = await _context.TrafficFines
                    .CountAsync(f =>
                        f.Status == FineStatus.PendingFinanceApproval),

                CompletedFines = await _context.TrafficFines
                    .CountAsync(f =>
                        f.Status == FineStatus.Completed),

                RejectedFines = await _context.TrafficFines
                    .CountAsync(f =>
                        f.Status == FineStatus.Rejected),

                TotalAmount = await _context.TrafficFines
                    .SumAsync(f => (decimal?)f.Amount) ?? 0m,

                RecentFines = await _context.TrafficFines
                    .AsNoTracking()
                    .Include(f => f.Vehicle)
                    .OrderByDescending(f => f.CreatedAt)
                    .ThenByDescending(f => f.Id)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}
