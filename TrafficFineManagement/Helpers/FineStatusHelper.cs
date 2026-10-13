
using TrafficFineManagement.Enums;

namespace TrafficFineManagement.Helpers
{
    public static class FineStatusHelper
    {
        public static string GetDisplayName(FineStatus status)
        {
            return status switch
            {
                FineStatus.New => "Yeni",
                FineStatus.PendingManagerApproval =>
                    "Yönetici Onayı Bekliyor",
                FineStatus.PendingFinanceApproval =>
                    "Finans Onayı Bekliyor",
                FineStatus.Completed => "Tamamlandı",
                FineStatus.Rejected => "Reddedildi",
                _ => "Bilinmeyen Durum"
            };
        }
    }
}
