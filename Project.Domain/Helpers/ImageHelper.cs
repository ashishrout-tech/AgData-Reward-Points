using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;


namespace Project.Domain.Helpers
{
    public static class ImageHelper
    {
        public static (byte[] thumbBytes, string thumbContentType) MakeThumbnail(
            byte[] original,
			int maxWidth = 320,
			int maxHeight = 320,
			int jpegQuality = 80
		)
		{

			if (original is null || original.Length == 0)
				throw new ArgumentException("Input image is empty.", nameof(original));


			using var inStream = new MemoryStream(original, writable: false);
			IImageFormat? detected = Image.DetectFormat(inStream);
			if(detected is null)
				throw new ArgumentException("Input image format is not recognized.", nameof(original));

			using Image image = Image.Load(inStream);
			image.Mutate(x => x.AutoOrient());
			image.Mutate(x => x.Resize(new ResizeOptions
			{
				Mode = ResizeMode.Max,
				Size = new Size(maxWidth, maxHeight),
				Sampler = KnownResamplers.Lanczos3
			}));

			using var outStream = new MemoryStream();
			
			var pngEncoder = new PngEncoder();
			image.Save(outStream, pngEncoder);
			return (outStream.ToArray(), "image/png");
		}

		public static byte[] ComputeCheckSum(byte[] content)
		{
			using var sha256 = System.Security.Cryptography.SHA256.Create();
			return sha256.ComputeHash(content);
		}
	}
}
