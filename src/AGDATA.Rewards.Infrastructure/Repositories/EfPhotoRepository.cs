using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Data;

namespace Project.Infrastructure.Repositories
{
    public class EfPhotoRepository : IPhotoRepository
    {
        private readonly AppDbContext _db;

        public EfPhotoRepository(AppDbContext db)
        {
            _db = db;
		}

        public async Task AddAsync(Photo photo, CancellationToken cancellationToken = default)
        {
            if (photo == null)
                throw new ArgumentNullException(nameof(photo));
            await _db.Photos.AddAsync(photo, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<Photo?> GetByIdOriginalAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Photos
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new Photo
                {
                    Id = p.Id,
                    FileName = p.FileName,
                    Content = p.Content,
                    ContentType = p.ContentType,
                    SizeBytes = p.SizeBytes,
                    CheckSum = p.CheckSum,
                    UploadedAt = p.UploadedAt
                    // Thumbnail intentionally excluded
                })
				.FirstOrDefaultAsync(cancellationToken);
		}

        public async Task<Photo?> GetByIdThumbAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _db.Photos
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new Photo
                {
                    Id = p.Id,
                    FileName = p.FileName,
                    Thumbnail = p.Thumbnail,
                    ContentType = p.ContentType,
                    SizeBytes = p.SizeBytes,
                    CheckSum = p.CheckSum,
                    UploadedAt = p.UploadedAt
                    // Content intentionally excluded (only Thumbnail is loaded)
                })
                .FirstOrDefaultAsync(cancellationToken);
		}

        public async Task<List<Photo>?> GetByIdsThumbAsync(List<Guid> ids, CancellationToken cancellationToken = default)
        {
            return await _db.Photos
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id))
                .Select(p => new Photo
                {
                    Id = p.Id,
                    FileName = p.FileName,
                    Thumbnail = p.Thumbnail,
                    ContentType = p.ContentType,
                    SizeBytes = p.SizeBytes,
                    CheckSum = p.CheckSum,
                    UploadedAt = p.UploadedAt
                    // Content intentionally excluded (only Thumbnail is loaded)
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<Photo?> GetByCheckSum(byte[] checksum, CancellationToken cancellationToken = default)
        {
            return await _db.Photos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CheckSum.SequenceEqual(checksum), cancellationToken);
		}
	}
}
