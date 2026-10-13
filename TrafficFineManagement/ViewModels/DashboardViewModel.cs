
using TrafficFineManagement.Models;

namespace TrafficFineManagement.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalVehicles { get; set; }
        public int TotalFines { get; set; }
        public int PendingManager { get; set; }
        public int PendingFinance { get; set; }
        public int CompletedFines { get; set; }
        public int RejectedFines { get; set; }

        public decimal TotalAmount { get; set; }

        public List<TrafficFine> RecentFines { get; set; }
            = new List<TrafficFine>();
    }
}
