using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;


namespace Project.Application.Services
{
    public class ProfilePictureGeneratorService : IProfilePictureGeneratorService
    {
        private readonly IPhotoService _photoService;
        public ProfilePictureGeneratorService(IPhotoService photoService)
        {
            _photoService = photoService;
        }
		public async Task<Guid> GenerateProfilePictureAsync(string name, CancellationToken cancellationToken = default)
        {
            const int size = 200;
            using var image = new Image<Rgba32>(size, size);

			var circle = new EllipsePolygon(size / 2f, size / 2f, size / 2f);
			image.Mutate(ctx =>
			{
				ctx.SetGraphicsOptions(new GraphicsOptions { Antialias = true });
				ctx.Fill(Color.CornflowerBlue, circle);
			});

			var font = SystemFonts.CreateFont("Arial", 80, FontStyle.Bold);
            var initials = string.Join("", name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(n => n[0])).ToUpper();



			var textOptions = new RichTextOptions(font)
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Origin = new PointF(size / 2f, size / 2f),
				WrappingLength = 0
			};

			image.Mutate(ctx =>
			{
				ctx.DrawText(textOptions, initials.ToUpperInvariant(), Color.White);
			});

			using var ms = new MemoryStream();
			image.Save(ms, new PngEncoder
			{
				ColorType = PngColorType.RgbWithAlpha,
				CompressionLevel = PngCompressionLevel.DefaultCompression
			});

			var fileName = $"{initials}_{DateTime.Now}_profile.png";

			var profilePhoto = await _photoService.UploadPhotoAsync(ms.ToArray(), fileName, "image/png", cancellationToken);

			return profilePhoto.Id;
		}
    }
}
