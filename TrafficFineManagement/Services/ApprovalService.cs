
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TrafficFineManagement.Data;
using TrafficFineManagement.Enums;
using TrafficFineManagement.Models;
using TrafficFineManagement.Seed;

namespace TrafficFineManagement.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ApprovalService(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApprovalResult> ProcessAsync(
            int trafficFineId,
            string userId,
            bool isManager,
            bool isFinance,
            ApprovalAction action,
            string? explanation)
        {
            // Kullanıcı gerçekten mevcut mu?
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new ApprovalResult(
                    false, "Kullanıcı bulunamadı.");
            }

            // Rolleri veritabanından doğrula.
            bool hasManagerRole = await _userManager
                .IsInRoleAsync(user, IdentitySeeder.ManagerRole);

            bool hasFinanceRole = await _userManager
                .IsInRoleAsync(user, IdentitySeeder.FinanceRole);

            // Sadece geçerli işlemlere izin ver.
            if (action != ApprovalAction.Approved &&
                action != ApprovalAction.Rejected)
            {
                return new ApprovalResult(
                    false, "Geçersiz işlem tipi.");
            }

            explanation = explanation?.Trim();

            if (action == ApprovalAction.Rejected &&
                string.IsNullOrWhiteSpace(explanation))
            {
                return new ApprovalResult(
                    false, "Ret gerekçesi zorunludur.");
            }

            if (explanation?.Length > 500)
            {
                return new ApprovalResult(
                    false, "Açıklama 500 karakteri geçemez.");
            }

            // İşlemlerin birlikte tamamlanması için transaction.
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var fine = await _context.TrafficFines
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == trafficFineId);

            if (fine == null)
            {
                return new ApprovalResult(
                    false, "Trafik cezası bulunamadı.");
            }

            FineStatus previousStatus = fine.Status;
            FineStatus newStatus;

            if (previousStatus == FineStatus.PendingManagerApproval)
            {
                if (!hasManagerRole || !isManager)
                {
                    return new ApprovalResult(
                        false, "Bu işlem için Yönetici yetkisi gerekli.");
                }

                newStatus = action == ApprovalAction.Approved
                    ? FineStatus.PendingFinanceApproval
                    : FineStatus.Rejected;
            }
            else if (previousStatus == FineStatus.PendingFinanceApproval)
            {
                if (!hasFinanceRole || !isFinance)
                {
                    return new ApprovalResult(
                        false, "Bu işlem için Finans yetkisi gerekli.");
                }

                newStatus = action == ApprovalAction.Approved
                    ? FineStatus.Completed
                    : FineStatus.Rejected;
            }
            else
            {
                return new ApprovalResult(
                    false,
                    "Bu ceza mevcut durumunda onaylanamaz veya reddedilemez.");
            }

            // Durum değişmemişse işlem yapma.
            // Eş zamanlı isteklerde yalnızca biri başarılı olabilir.
            int updatedRows = await _context.TrafficFines
                .Where(f => f.Id == trafficFineId &&
                            f.Status == previousStatus)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.Status, newStatus));

            if (updatedRows != 1)
            {
                return new ApprovalResult(
                    false,
                    "Ceza durumu başka bir işlem tarafından değiştirilmiş.");
            }

            var history = new ApprovalHistory
            {
                TrafficFineId = trafficFineId,
                UserId = userId,
                ActionDate = DateTime.UtcNow,
                Action = action,
                Explanation = explanation,
                PreviousStatus = previousStatus,
                NewStatus = newStatus
            };

            _context.ApprovalHistories.Add(history);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            string message = action == ApprovalAction.Rejected
                ? "Trafik cezası reddedildi."
                : newStatus == FineStatus.Completed
                    ? "Trafik cezasının onay süreci tamamlandı."
                    : "Ceza onaylandı ve Finans aşamasına gönderildi.";

            return new ApprovalResult(true, message);
        }
    }
}
