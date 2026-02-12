using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Domain.Entities;

namespace Project.Domain.Interfaces
{
    public interface IPhotoRepository
    {
        Task AddAsync(Photo photo, CancellationToken cancellationToken = default);
        Task<Photo?> GetByIdOriginalAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Photo?> GetByCheckSum(byte[] checksum, CancellationToken cancellationToken = default);
		Task<Photo?> GetByIdThumbAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Photo>?> GetByIdsThumbAsync(List<Guid> ids, CancellationToken cancellationtToken = default);
	}
}
