using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace RaceDay.API.Services;
public class BlobStorageService(IConfiguration configuration)
{
    private BlobServiceClient CreateClient()
    {
        var connection = configuration["AzureBlobStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connection)) throw new InvalidOperationException("Azure Blob Storage is not configured. Set AzureBlobStorage__ConnectionString.");
        return new BlobServiceClient(connection);
    }

    public async Task<string> UploadAsync(IFormFile file,string containerName,string prefix,CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(extension)) throw new ArgumentException("Only JPG, PNG and WEBP images are accepted.");
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024) throw new ArgumentException("Image size must be between 1 byte and 5 MB.");
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("The uploaded file must be an image.");
        var safeName = $"{prefix}-{Guid.NewGuid():N}{extension}";
        var container = CreateClient().GetBlobContainerClient(containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None,cancellationToken: cancellationToken);
        var blob = container.GetBlobClient(safeName);
        await using var stream = file.OpenReadStream();
        await blob.UploadAsync(stream,new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType } },cancellationToken);
        return safeName;
    }

    public async Task<(Stream Content,string ContentType)> DownloadAsync(string containerName,string blobName,CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(blobName) || blobName.Contains('/') || blobName.Contains('\\') || blobName.Contains(".."))
            throw new ArgumentException("Invalid image name.");
        var blob = CreateClient().GetBlobContainerClient(containerName).GetBlobClient(blobName);
        var response = await blob.DownloadStreamingAsync(cancellationToken: cancellationToken);
        var contentType = response.Value.Details.ContentType ?? "application/octet-stream";
        return (response.Value.Content,contentType);
    }
}
