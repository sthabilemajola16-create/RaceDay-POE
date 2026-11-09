using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Auth;

namespace RaceDay.Api.Controllers;

/// <summary>Organiser-only upload of event artwork to Azure Blob Storage.</summary>
[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private readonly IConfiguration _configuration;
    public UploadsController(IConfiguration configuration) => _configuration = configuration;

    /// <summary>Uploads an event image. Configure AzureBlob:ConnectionString and AzureBlob:Container in settings.</summary>
    [HttpPost("event-image")]
    [SessionAuthorize(Roles.Organiser)]
    [RequestSizeLimit(5_000_000)]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> EventImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "Choose an image file." });
        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only JPG, PNG and WEBP images are accepted." });
        var connection = _configuration["AzureBlob:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connection))
            return StatusCode(503, new { message = "Azure Blob Storage is not configured. Set AzureBlob:ConnectionString." });

        var containerName = _configuration["AzureBlob:Container"] ?? "raceday-event-images";
        var service = new BlobServiceClient(connection);
        var container = service.GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        var extension = Path.GetExtension(Path.GetFileName(file.FileName)).ToLowerInvariant();
        var blob = container.GetBlobClient($"{Guid.NewGuid():N}{extension}");
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType }, cancellationToken: cancellationToken);
        return Created(blob.Uri.ToString(), new { url = blob.Uri.ToString(), fileName = blob.Name });
    }
}
