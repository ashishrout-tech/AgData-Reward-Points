using Project.Application.DTOs.Redemption;
using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public interface IRedemptionService
    {
        Task<RedemptionDetailDto> CreateRedemptionAsync(Guid userId, CreateRedemptionRequest request, CancellationToken cancellationToken = default);
        Task<List<RedemptionDto>> GetPendingRedemptionsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<List<RedemptionDto>> GetUserRedemptionsAsync(Guid userId, int? status = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<RedemptionDetailDto> GetRedemptionAsync(Guid id, CancellationToken cancellationToken = default);
        Task<RedemptionApproveResponseDto> ApproveRedemptionAsync(Guid id, Guid adminId, CancellationToken cancellationToken = default);
        Task<RedemptionRejectResponseDto> RejectRedemptionAsync(Guid id, RejectRedemptionRequest request, Guid adminId, CancellationToken cancellationToken = default);
        Task<List<RedemptionDto>> GetRedemptionsByProductAsync(Guid productId, int? status = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
        Task<RedemptionStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default);
        Task<List<RejectedRedemptionReportDto>> GetRejectedRedemptionsReportAsync(string? reason = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default);
    }
}
