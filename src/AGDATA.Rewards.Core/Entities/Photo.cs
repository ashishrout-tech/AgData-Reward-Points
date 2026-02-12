using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Project.Domain.Helpers.ImageHelper;

namespace Project.Domain.Entities
{
    public class Photo
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = default!;
        public byte[] Content { get; set; } = default!;
        public byte[]? Thumbnail { get; set; }
		public string ContentType { get; set; } = default!;
        public long SizeBytes { get; set; }
        public byte[] CheckSum { get; set; } = default!;
        public DateTime UploadedAt { get; set; }

        public Photo() { }
		public Photo(string fileName, byte[] content, string contentType, byte[] checksum, byte[]? thumbnail = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("FileName cannot be null or empty.", nameof(fileName));
            if (content == null || content.LongLength == 0L || content.LongLength > 15L * 1024 * 1024)
                throw new ArgumentException("Not applicable content length", nameof(content));
            if (string.IsNullOrWhiteSpace(contentType))
                throw new ArgumentException("ContentType cannot be null or empty.", nameof(contentType));
            Id = Guid.NewGuid();
            FileName = fileName;
            Content = content;
            ContentType = contentType;
            SizeBytes = content.Length;
            Thumbnail = thumbnail;
            CheckSum = checksum;
            UploadedAt = DateTime.UtcNow;
		}
	}
}
