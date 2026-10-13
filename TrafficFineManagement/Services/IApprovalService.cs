
using TrafficFineManagement.Enums;

namespace TrafficFineManagement.Services
{
    public interface IApprovalService
    {
        Task<ApprovalResult> ProcessAsync(
            int trafficFineId,
            string userId,
            bool isManager,
            bool isFinance,
            ApprovalAction action,
            string? explanation);
    }

    public record ApprovalResult(
        bool Succeeded,
        string Message);
}
