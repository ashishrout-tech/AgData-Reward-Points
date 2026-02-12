using AutoMapper;
using Project.Domain.Entities.Users;
using Project.Application.DTOs.User;
using Project.Domain.Entities.Product;
using Project.Application.DTOs.Product;
using Project.Domain.Entities.Event;
using Project.Application.DTOs.Event;
using Project.Domain.Entities;
using Project.Application.DTOs.Transaction;
using Project.Application.DTOs.Redemption;
using UserDto = Project.Application.DTOs.User.UserDto;
using EventDto = Project.Application.DTOs.Event.EventDto;
using Project.Application.DTOs.Photo;

namespace Project.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.UserAccount, opt => opt.MapFrom(src => src.UserAccount));
            
            // UserAccount mappings
            CreateMap<UserAccount, UserAccountDto>();

            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.PointsPrice, opt => opt.MapFrom(src => src.ProductPrice.CurrentPoints))
                .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src => src.ProductStock.AvailableStock));

            CreateMap<Product, ProductDetailDto>()
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.ProductPrice))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.ProductStock));

            CreateMap<ProductPrice, ProductPriceDto>();
            CreateMap<ProductStock, ProductStockDto>();

            // Event mappings
            CreateMap<Event, EventDto>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.EventSchedule.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EventSchedule.EndTime))
                .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => src.IsCancelled));

            CreateMap<EventParticipant, EventParticipantDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            CreateMap<EventParticipant, EventParticipantDetailDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));

            // Transaction mappings
            CreateMap<Transaction, TransactionDto>();
            CreateMap<Transaction, TransactionDetailDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Event, opt => opt.MapFrom(src => src.Event));

            CreateMap<User, Project.Application.DTOs.Transaction.TransactionUserDto>();
            CreateMap<Event, Project.Application.DTOs.Transaction.TransactionEventDto>();

			// Redemption mappings
			CreateMap<User, UserBasicDto>();
			CreateMap<Redemption, RedemptionDetailDto>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Product, opt => opt.MapFrom(src => src.Product))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovalAdmin.Name : null))
                .ForMember(dest => dest.RejectedByName, opt => opt.MapFrom(src => src.RejectedBy != null ? src.RejectionAdmin.Name : null));

			CreateMap<Redemption, RedemptionDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Name))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.PointsRequired, opt => opt.MapFrom(src => (int)src.Product.ProductPrice.CurrentPoints));

            CreateMap<Product, Project.Application.DTOs.Redemption.ProductBasicDto>()
                .ForMember(dest => dest.PointsRequired, opt => opt.MapFrom(src => (int)src.ProductPrice.CurrentPoints));

            CreateMap<Photo, PhotoOriginalDto>();
            CreateMap<Photo, PhotoThumbDto>();
		}
    }
}
