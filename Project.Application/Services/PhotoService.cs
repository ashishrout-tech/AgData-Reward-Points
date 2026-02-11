using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Project.Application.DTOs.Photo;
using Project.Domain.Entities;
using Project.Domain.Helpers;
using Project.Domain.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Project.Application.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly ILogger<PhotoService> _logger;
        private readonly IMapper _mapper;

        public PhotoService(
            IPhotoRepository photoRepository,
            ILogger<PhotoService> logger,
            IMapper mapper)
        {
            _photoRepository = photoRepository;
            _logger = logger;
            _mapper = mapper;
		}

        public async Task<PhotoUploadResponseDto> UploadPhotoAsync(byte[] content, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
				if (content is null || content.Length == 0)
					throw new ArgumentException("Empty content", nameof(content));
				if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
					throw new ArgumentException("Only images are allowed", nameof(contentType));

				// App-level max size (example: 15 MB)
				const long maxSize = 15L * 1024 * 1024;
				if (content.LongLength > maxSize)
					throw new InvalidOperationException($"Image too large. Max {maxSize / (1024 * 1024)} MB.");

				var bgRemovedPng = StripBackgroundToPng(content, hardCut: 40f, feather: 20f);


				var checksum = ImageHelper.ComputeCheckSum(bgRemovedPng);
				if(!PhotoExistsAsync(checksum, cancellationToken).Result)
				{
					var (thumbBytes, thumbContentType) = ImageHelper.MakeThumbnail(bgRemovedPng, 320, 320, 80);

					var entity = new Photo(fileName, bgRemovedPng, "image/png", checksum, thumbBytes);

					await _photoRepository.AddAsync(entity, cancellationToken);
					_logger.LogInformation("Photo uploaded successfully with ID: {PhotoId}", entity.Id);
					return new PhotoUploadResponseDto
					{
						Id = entity.Id,
						FileName = entity.FileName,
						UploadedAt = entity.UploadedAt,
						ContentType = entity.ContentType,
					};
				}
				else
				{
					var existingPhoto = await _photoRepository.GetByCheckSum(checksum, cancellationToken);
					return new PhotoUploadResponseDto
					{
						Id = existingPhoto.Id,
						FileName = existingPhoto.FileName,
						UploadedAt = existingPhoto.UploadedAt,
						ContentType = existingPhoto.ContentType,
					};
				}
				
				
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error uploading photo: {Message}", ex.Message);
				throw new Exception("Failed to upload photo", ex);
			}
		}

		public async Task<PhotoOriginalDto> GetOriginalAsync(
				Guid photoId, CancellationToken ct = default)
		{
			var item = await _photoRepository.GetByIdOriginalAsync(photoId, ct);
			if (item == null)
			{
				throw new KeyNotFoundException($"Photo with ID {photoId} not found.");
			}

            return _mapper.Map<PhotoOriginalDto>(item);
		}

		public async Task<PhotoThumbDto> GetThumbnailAsync(
				Guid photoId, CancellationToken ct = default)
		{
			var item = await _photoRepository.GetByIdThumbAsync(photoId, ct);
			if (item == null)
			{
				throw new KeyNotFoundException($"Photo with ID {photoId} not found.");
			}

			return _mapper.Map<PhotoThumbDto>(item);
		}

		public async Task<List<PhotoThumbDto>> GetThumbnailsByIdsAsync(List<Guid> photoIds, CancellationToken ct = default)
		{
			var items = await _photoRepository.GetByIdsThumbAsync(photoIds, ct);
			if (items == null || items.Count == 0)
			{
				throw new KeyNotFoundException("No photos found for the provided IDs.");
			}

			return _mapper.Map<List<PhotoThumbDto>>(items);
		}

		private async Task<bool> PhotoExistsAsync(byte[] checksum, CancellationToken cancellationToken = default)
		{
			var existingPhoto = await _photoRepository.GetByCheckSum(checksum, cancellationToken);
			return existingPhoto != null;
		}



		private static byte[] StripBackgroundToPng(byte[] input, float hardCut = 40f, float feather = 20f)
		{
			using var srcStream = new MemoryStream(input, writable: false);
			using var image = Image.Load<Rgba32>(srcStream);

			image.Mutate(x => x.AutoOrient());

			var bg = EstimateBackgroundColor(image); // same as before

			float softCut = hardCut + feather;

			image.ProcessPixelRows(accessor =>
			{
				for (int y = 0; y < accessor.Height; y++)
				{
					var row = accessor.GetRowSpan(y);
					for (int x = 0; x < row.Length; x++)
					{
						var px = row[x];

						// Euclidean distance in sRGB 0..255 space
						float dr = px.R - bg.R;
						float dg = px.G - bg.G;
						float db = px.B - bg.B;
						float dist = MathF.Sqrt(dr * dr + dg * dg + db * db); // range ~ 0..441

						if (dist <= hardCut)
						{
							row[x] = new Rgba32(px.R, px.G, px.B, 0); // fully transparent
						}
						else if (dist < softCut)
						{
							// Feather alpha 0..1 over the soft band
							float t = (dist - hardCut) / feather; // 0..1
							byte a = (byte)Math.Clamp(px.A * t, 0, 255);
							row[x].A = a;
						}
					}
				}
			});

			using var outStream = new MemoryStream();
			image.Save(outStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder
			{
				ColorType = SixLabors.ImageSharp.Formats.Png.PngColorType.RgbWithAlpha
			});
			return outStream.ToArray();
		}


		private static Rgba32 EstimateBackgroundColor(Image<Rgba32> img, int stepsPerEdge = 16)
		{
			var samplesR = new List<byte>(stepsPerEdge * 4);
			var samplesG = new List<byte>(stepsPerEdge * 4);
			var samplesB = new List<byte>(stepsPerEdge * 4);

			int w = img.Width, h = img.Height;

			img.ProcessPixelRows(accessor =>
			{
				for (int i = 0; i < stepsPerEdge; i++)
				{
					int x = (int)MathF.Round((i + 0.5f) * (w - 1) / stepsPerEdge);
					int y = (int)MathF.Round((i + 0.5f) * (h - 1) / stepsPerEdge);

					var top = accessor.GetRowSpan(0)[x];
					var bot = accessor.GetRowSpan(h - 1)[x];
					var left = accessor.GetRowSpan(y)[0];
					var right = accessor.GetRowSpan(y)[w - 1];

					samplesR.Add(top.R); samplesG.Add(top.G); samplesB.Add(top.B);
					samplesR.Add(bot.R); samplesG.Add(bot.G); samplesB.Add(bot.B);
					samplesR.Add(left.R); samplesG.Add(left.G); samplesB.Add(left.B);
					samplesR.Add(right.R); samplesG.Add(right.G); samplesB.Add(right.B);
				}
			});

			byte mr = Median(samplesR);
			byte mg = Median(samplesG);
			byte mb = Median(samplesB);
			return new Rgba32(mr, mg, mb, 255);

			static byte Median(List<byte> vals)
			{
				vals.Sort();
				int mid = vals.Count / 2;
				return vals.Count % 2 == 1
					? vals[mid]
					: (byte)((vals[mid - 1] + vals[mid]) / 2);
			}
		}

		/// <summary>
		/// Convert sRGB 0..255 to linear 0..1 for perceptually saner distances.
		/// </summary>
		private static Vector3 ToLinear(Rgba32 rgba)
		{
			static float SrgbToLinear(byte c)
			{
				float v = c / 255f;
				return v <= 0.04045f ? v / 12.92f : MathF.Pow((v + 0.055f) / 1.055f, 2.4f);
			}
			return new Vector3(SrgbToLinear(rgba.R), SrgbToLinear(rgba.G), SrgbToLinear(rgba.B));
		}


	}
}
