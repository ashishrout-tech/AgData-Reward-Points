using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Project.Application.DTOs.Photo;
using Project.Application.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Project.API.Controllers
{
	/// <summary>
	/// Manages photo uploads and retrieval operations.
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	[Authorize]
	public class PhotosController : ControllerBase
	{
		private readonly IPhotoService _photoService;
		private readonly ILogger<PhotosController> _logger;

		public PhotosController(IPhotoService photoService, ILogger<PhotosController> logger)
		{
			_photoService = photoService;
			_logger = logger;
		}

		/// <summary>
		/// Uploads a new photo with automatic thumbnail generation.
		/// </summary>
		/// <param name="file">The image file to upload (max 25 MB).</param>
		/// <param name="ct">Cancellation token.</param>
		/// <returns>Returns 200 OK if the upload is successful.</returns>
		/// <response code="200">Photo uploaded successfully.</response>
		/// <response code="400">Invalid file or no file provided. Only image files are accepted.</response>
		/// <response code="401">Unauthorized - authentication required.</response>
		/// <response code="500">Internal server error occurred during upload.</response>
		[HttpPost("upload")]
		[Consumes("multipart/form-data")]
		[RequestSizeLimit(25_000_000)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<IActionResult> Upload([FromForm] IFormFile file, CancellationToken ct)
		{
			try
			{
				if (file is null || file.Length == 0)
					return BadRequest("No file uploaded.");
				if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
					return BadRequest("Only images are allowed.");


				byte[] content;
				await using (var ms = new MemoryStream())
				{
					await file.CopyToAsync(ms, ct);
					content = ms.ToArray();
				}

				_logger.LogInformation("Uploading photo: {FileName}, Size: {Size} bytes", file.FileName, file.Length);
				var response = await _photoService.UploadPhotoAsync(content, file.FileName, file.ContentType, ct);
				return Ok(response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error uploading photo");
				return StatusCode(500, "An error occurred while uploading the photo.");
			}
		}

		/// <summary>
		/// Retrieves the original full-size photo by ID.
		/// </summary>
		/// <param name="id">The unique identifier of the photo.</param>
		/// <param name="ct">Cancellation token.</param>
		/// <returns>Returns the original photo with full content and metadata.</returns>
		/// <response code="200">Photo retrieved successfully.</response>
		/// <response code="401">Unauthorized - authentication required.</response>
		/// <response code="404">Photo not found.</response>
		/// <response code="500">Internal server error occurred during retrieval.</response>
		[HttpGet("{id}")]
		[ProducesResponseType(typeof(PhotoOriginalDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<PhotoOriginalDto>> GetOriginalPhoto(Guid id, CancellationToken ct)
		{
			try
			{
				var photo = await _photoService.GetOriginalAsync(id, ct);
				if (photo is null)
					return NotFound();
				_logger.LogInformation("Retrieved photo: {FileName}, Size: {Size} bytes", photo.FileName, photo.SizeBytes);
				return Ok(photo);

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving photo with ID: {PhotoId}", id);
				return StatusCode(500, "An error occurred while retrieving the photo.");
			}
		}

		/// <summary>
		/// Retrieves the thumbnail version of a photo by ID.
		/// </summary>
		/// <param name="id">The unique identifier of the photo.</param>
		/// <param name="ct">Cancellation token.</param>
		/// <returns>Returns the thumbnail image with reduced size for preview purposes.</returns>
		/// <response code="200">Thumbnail retrieved successfully.</response>
		/// <response code="401">Unauthorized - authentication required.</response>
		/// <response code="404">Thumbnail not found.</response>
		/// <response code="500">Internal server error occurred during retrieval.</response>
		[HttpGet("{id}/thumb")]
		[ProducesResponseType(typeof(PhotoThumbDto), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<PhotoThumbDto>> GetThumbnail(Guid id, CancellationToken ct)
		{
			try
			{
				var photo = await _photoService.GetThumbnailAsync(id, ct);
				if (photo is null)
					return NotFound();
				_logger.LogInformation("Retrieved thumbnail for photo ID: {PhotoId}", id);
				return Ok(photo);
			}
			catch
			{
				_logger.LogError("Error retrieving thumbnail for photo with ID: {PhotoId}", id);
				return StatusCode(500, "An error occurred while retrieving the thumbnail.");
			}
		}

		/// <summary>
		/// Retrieves multiple thumbnails by a list of photo IDs.
		/// </summary>
		/// <param name="ids">List of photo IDs to retrieve thumbnails for.</param>
		/// <param name="ct">Cancellation token.</param>
		/// <returns>Returns a list of thumbnail images for the requested photo IDs.</returns>
		/// <response code="200">Thumbnails retrieved successfully.</response>
		/// <response code="400">Invalid request body.</response>
		/// <response code="401">Unauthorized - authentication required.</response>
		/// <response code="404">No thumbnails found for the provided IDs.</response>
		/// <response code="500">Internal server error occurred during retrieval.</response>
		[HttpPost("thumbnailList")]
		[ProducesResponseType(typeof(List<PhotoThumbDto>), StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<List<PhotoThumbDto>>> GetThumbnailsList([FromBody] List<Guid> ids, CancellationToken ct)
		{
			try
			{
				if (ids == null || ids.Count == 0)
					return BadRequest("No photo IDs provided.");

				var items = await _photoService.GetThumbnailsByIdsAsync(ids, ct);
				if(items == null || items.Count == 0)
					return NotFound();
				return Ok(items);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error retrieving thumbnails for photo IDs: {PhotoIds}", string.Join(", ", ids));
				return StatusCode(500, "An error occurred while retrieving the thumbnails.");
			}
		}
	}
}
