using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Application.DTOs.Photo;
using Project.Domain.Entities;

namespace Project.Application.Services
{
    public interface IPhotoService
    {
        Task<PhotoUploadResponseDto> UploadPhotoAsync(byte[] content, string fileName, string contentType, CancellationToken cancellationToken = default);
        Task<PhotoOriginalDto> GetOriginalAsync(Guid photoId, CancellationToken cancellationToken = default);
        Task<PhotoThumbDto> GetThumbnailAsync(Guid photoId, CancellationToken cancellationToken = default);
        Task<List<PhotoThumbDto>> GetThumbnailsByIdsAsync(List<Guid> photoIds, CancellationToken cancellationToken = default);
	}
}
