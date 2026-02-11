using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs.Photo
{
    public class PhotoOriginalDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
	}

    public class PhotoThumbDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public byte[] Thumbnail { get; set; } = Array.Empty<byte>();
        public DateTime UploadedAt { get; set; }
	}

    public class PhotoUploadResponseDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public string ContentType { get; set; } = string.Empty;
	}
}
