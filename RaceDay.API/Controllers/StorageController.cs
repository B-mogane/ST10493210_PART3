using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.API.DTOs;
using RaceDay.API.Services;
namespace RaceDay.API.Controllers;
[ApiController]
[Route("api/storage")]
public class StorageController(BlobStorageService storage) : ControllerBase
{
    [HttpPost("event-banner"), Authorize(Roles = "Organiser")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadEventBanner(IFormFile? file,CancellationToken cancellationToken)
        => await Upload(file,"event-banners","event",cancellationToken);

    [HttpPost("profile-picture"), Authorize]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile? file,CancellationToken cancellationToken)
        => await Upload(file,"profile-pictures","profile",cancellationToken);

    private async Task<IActionResult> Upload(IFormFile? file,string container,string prefix,CancellationToken ct)
    {
        if (file is null) return BadRequest(new { message = "Select an image file." });
        try
        {
            var name = await storage.UploadAsync(file,container,prefix,ct);
            return Ok(new StorageUploadResponse(container,name,file.ContentType));
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return StatusCode(503,new { message = ex.Message }); }
        catch (Exception) { return StatusCode(502,new { message = "The image could not be uploaded to cloud storage." }); }
    }

    // Images are served through the API. Containers remain private in Azure; no public blob access is required.
    [HttpGet("images/{container}/{blobName}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(string container,string blobName,CancellationToken cancellationToken)
    {
        if (container is not ("event-banners" or "profile-pictures")) return NotFound();
        try
        {
            var image = await storage.DownloadAsync(container,blobName,cancellationToken);
            return File(image.Content,image.ContentType);
        }
        catch (Azure.RequestFailedException) { return NotFound(); }
        catch (ArgumentException) { return BadRequest(); }
        catch (InvalidOperationException) { return StatusCode(503); }
    }
}
