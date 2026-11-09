using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
namespace RaceDay.MVC.Controllers;
public class MediaController(RaceDayApiClient api) : Controller
{
    [HttpGet("media/{container}/{blobName}")]
    public async Task<IActionResult> Image(string container,string blobName)
    {
        if (container is not ("event-banners" or "profile-pictures")) return NotFound();
        try { var image = await api.GetImageAsync(container,blobName); return File(image.Bytes,image.ContentType); }
        catch { return NotFound(); }
    }
}
