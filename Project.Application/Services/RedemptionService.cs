using AutoMapper;
using Microsoft.Extensions.Logging;
using Project.Application.DTOs.Redemption;
using Project.Domain.Entities;
using Project.Domain.Enums;
using Project.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public class RedemptionService : IRedemptionService
    {
        private readonly IRedemptionAsyncRepository _redemptionRepo;
        private readonly IProductAsyncRepository _productRepo;
        private readonly IUserAccountAsyncRepository _userAccountRepo;
        private readonly ITransactionAsyncRepository _transactionRepo;
        private readonly IMapper _mapper;
        private readonly ILogger<RedemptionService> _logger;

        public RedemptionService(
            IRedemptionAsyncRepository redemptionRepo,
            IProductAsyncRepository productRepo,
            IUserAccountAsyncRepository userAccountRepo,
            ITransactionAsyncRepository transactionRepo,
            IMapper mapper,
            ILogger<RedemptionService> logger)
        {
            _redemptionRepo = redemptionRepo;
            _productRepo = productRepo;
            _userAccountRepo = userAccountRepo;
            _transactionRepo = transactionRepo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<RedemptionDetailDto> CreateRedemptionAsync(Guid userId, CreateRedemptionRequest request, CancellationToken cancellationToken = default)
        {
            var product = await _productRepo.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                throw new KeyNotFoundException($"Product {request.ProductId} not found");

            var userAccount = await _userAccountRepo.GetAccountByUserIdAsync(userId, cancellationToken);
            if (userAccount == null || userAccount.Points < (int)product.ProductPrice.CurrentPoints)
                throw new InvalidOperationException("Insufficient points for this redemption");

            var redemption = new Redemption(userId, request.ProductId);
            await _redemptionRepo.AddAsync(redemption, cancellationToken);

            _logger.LogInformation($"Redemption created: UserId={userId}, ProductId={request.ProductId}, RedemptionId={redemption.Id}");
            return _mapper.Map<RedemptionDetailDto>(redemption);
        }

        public async Task<List<RedemptionDto>> GetPendingRedemptionsAsync(int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var redemptions = await _redemptionRepo.GetPendingAsync(skip, take, cancellationToken);
            return redemptions.Select(r => new RedemptionDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.Name ?? "Unknown",
                UserEmail = r.User?.Email ?? "Unknown",
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? "Unknown",
                PointsRequired = (int?)r.Product?.ProductPrice.CurrentPoints ?? 0,
                Status = (int)r.Status,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<List<RedemptionDto>> GetUserRedemptionsAsync(Guid userId, int? status = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            List<Redemption> redemptions;
            
            if (status.HasValue)
                redemptions = await _redemptionRepo.GetUserRedemptionsByStatusAsync(userId, status, skip, take, cancellationToken);
            else
                redemptions = await _redemptionRepo.GetByUserIdAsync(userId, cancellationToken);

            return redemptions.Select(r => new RedemptionDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = "User",
                UserEmail = "N/A",
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? "Unknown",
                PointsRequired = (int?)r.Product?.ProductPrice.CurrentPoints ?? 0,
                Status = (int)r.Status,
                CreatedAt = r.CreatedAt,
                ApprovedOn = r.ApprovedOn,
                RejectedOn = r.RejectedOn
            }).ToList();
        }

        public async Task<RedemptionDetailDto> GetRedemptionAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var redemption = await _redemptionRepo.GetByIdAsync(id, cancellationToken);
            if (redemption == null)
                throw new KeyNotFoundException($"Redemption {id} not found");

            return _mapper.Map<RedemptionDetailDto>(redemption);
        }

        public async Task<RedemptionApproveResponseDto> ApproveRedemptionAsync(Guid id, Guid adminId, CancellationToken cancellationToken = default)
        {
            var redemption = await _redemptionRepo.GetByIdAsync(id, cancellationToken);
            if (redemption == null)
                throw new KeyNotFoundException($"Redemption {id} not found");

            if (redemption.Status != RedemptionStatus.Pending)
                throw new InvalidOperationException("Redemption can only be approved from Pending status");

            var product = await _productRepo.GetByIdAsync(redemption.ProductId, cancellationToken);
            var points = (int)product.ProductPrice.CurrentPoints;

            redemption.Approve(adminId);
            await _redemptionRepo.UpdateAsync(redemption, cancellationToken);

            await _userAccountRepo.UpdateAccountAsync(redemption.UserId, -points, cancellationToken);

            var transaction = Transaction.CreateRedemptionRefund(
                redemption.UserId,
                points,
                $"Redeemed product: {product.Name}",
                adminId);
            
            await _transactionRepo.AddAsync(transaction, cancellationToken);

            _logger.LogInformation($"Redemption approved: RedemptionId={id}, UserId={redemption.UserId}, AdminId={adminId}, Points={points}");

            return new RedemptionApproveResponseDto
            {
                Id = redemption.Id,
                UserId = redemption.UserId,
                ProductId = redemption.ProductId,
                ProductName = product.Name,
                PointsRequired = points,
                Status = (int)redemption.Status,
                CreatedAt = redemption.CreatedAt,
                ApprovedBy = redemption.ApprovedBy ?? Guid.Empty,
                ApprovedOn = redemption.ApprovedOn ?? DateTime.UtcNow
            };
        }

        public async Task<RedemptionRejectResponseDto> RejectRedemptionAsync(Guid id, RejectRedemptionRequest request, Guid adminId, CancellationToken cancellationToken = default)
        {
            var redemption = await _redemptionRepo.GetByIdAsync(id, cancellationToken);
            if (redemption == null)
                throw new KeyNotFoundException($"Redemption {id} not found");

            if (redemption.Status != RedemptionStatus.Pending)
                throw new InvalidOperationException("Redemption can only be rejected from Pending status");

            redemption.Reject(adminId, request.Reason);
            await _redemptionRepo.UpdateAsync(redemption, cancellationToken);

            RefundTransactionDto? refundTransaction = null;

            if (request.RefundPoints)
            {
                var product = await _productRepo.GetByIdAsync(redemption.ProductId, cancellationToken);
                var points = (int)product.ProductPrice.CurrentPoints;

                var transaction = Transaction.CreateRedemptionRefund(
                    redemption.UserId,
                    points,
                    $"Redemption refund - {request.Reason}",
                    adminId);
                
                await _transactionRepo.AddAsync(transaction, cancellationToken);

                await _userAccountRepo.UpdateAccountAsync(redemption.UserId, points, cancellationToken);

                refundTransaction = new RefundTransactionDto
                {
                    Id = transaction.Id,
                    Points = points,
                    Reason = transaction.Reason,
                    Source = (int)transaction.Source
                };
            }

            _logger.LogInformation($"Redemption rejected: RedemptionId={id}, UserId={redemption.UserId}, AdminId={adminId}, Reason={request.Reason}");

            return new RedemptionRejectResponseDto
            {
                Id = redemption.Id,
                UserId = redemption.UserId,
                ProductId = redemption.ProductId,
                Status = (int)redemption.Status,
                CreatedAt = redemption.CreatedAt,
                RejectedBy = redemption.RejectedBy ?? Guid.Empty,
                RejectedOn = redemption.RejectedOn ?? DateTime.UtcNow,
                RejectionReason = request.Reason,
                RefundedTransaction = refundTransaction
            };
        }

        public async Task<List<RedemptionDto>> GetRedemptionsByProductAsync(Guid productId, int? status = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var redemptions = await _redemptionRepo.GetRedemptionsByProductAsync(productId, status, skip, take, cancellationToken);
            return redemptions.Select(r => new RedemptionDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.Name ?? "Unknown",
                UserEmail = r.User?.Email ?? "Unknown",
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? "Unknown",
                PointsRequired = (int?)r.Product?.ProductPrice.CurrentPoints ?? 0,
                Status = (int)r.Status,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<RedemptionStatisticsDto> GetStatisticsAsync(CancellationToken cancellationToken = default)
        {
            var totalCount = await _redemptionRepo.GetRedemptionCountByStatusAsync((int)RedemptionStatus.Approved, cancellationToken) +
                             await _redemptionRepo.GetRedemptionCountByStatusAsync((int)RedemptionStatus.Pending, cancellationToken) +
                             await _redemptionRepo.GetRedemptionCountByStatusAsync((int)RedemptionStatus.Rejected, cancellationToken);

            var pendingCount = await _redemptionRepo.GetRedemptionCountByStatusAsync((int)RedemptionStatus.Pending, cancellationToken);
            var approvedCount = await _redemptionRepo.GetRedemptionCountByStatusAsync((int)RedemptionStatus.Approved, cancellationToken);
            var rejectedCount = await _redemptionRepo.GetRedemptionCountByStatusAsync((int)RedemptionStatus.Rejected, cancellationToken);

            var totalRedeemed = await _redemptionRepo.GetTotalPointsRedeemedAsync(cancellationToken);
            var totalRefunded = await _redemptionRepo.GetTotalPointsRefundedAsync(cancellationToken);
            var topProducts = await _redemptionRepo.GetTopProductsAsync(1, cancellationToken);

            var conversionRate = totalCount > 0 ? (approvedCount / (double)totalCount) * 100 : 0;

            return new RedemptionStatisticsDto
            {
                TotalRedemptions = totalCount,
                PendingCount = pendingCount,
                ApprovedCount = approvedCount,
                RejectedCount = rejectedCount,
                TotalPointsRedeemed = totalRedeemed,
                TotalPointsRefunded = totalRefunded,
                ConversionRate = conversionRate,
                TopProduct = topProducts.FirstOrDefault() != default 
                    ? new TopProductDto 
                    { 
                        ProductId = topProducts[0].ProductId, 
                        ProductName = topProducts[0].ProductName, 
                        RedemptionCount = topProducts[0].RedemptionCount 
                    }
                    : null
            };
        }

        public async Task<List<RejectedRedemptionReportDto>> GetRejectedRedemptionsReportAsync(string? reason = null, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var redemptions = await _redemptionRepo.GetRejectedRedemptionsAsync(reason, skip, take, cancellationToken);
            return redemptions.Select(r => new RejectedRedemptionReportDto
            {
                Id = r.Id,
                UserId = r.UserId,
                UserName = r.User?.Name ?? "Unknown",
                ProductId = r.ProductId,
                ProductName = r.Product?.Name ?? "Unknown",
                Reason = r.RejectionReason ?? "No reason provided",
                RejectedBy = r.RejectedBy ?? Guid.Empty,
                RejectedOn = r.RejectedOn ?? DateTime.UtcNow,
                Refunded = true,
                RefundAmount = (int?)r.Product?.ProductPrice.CurrentPoints ?? 0
            }).ToList();
        }
    }
}
