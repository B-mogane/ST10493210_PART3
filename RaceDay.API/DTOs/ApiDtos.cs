using System.ComponentModel.DataAnnotations;
namespace RaceDay.API.DTOs;
public record EventRequest([Required, MaxLength(100)] string EventName,string? Description,DateTime EventDate,[Required, MaxLength(200)] string Location,string? RouteInfo,decimal EntryFee,[Required] string Status,decimal DistanceKm = 0,[Required] string EventType = "Run",string? BannerImageName = null);
public record CategoryRequest([Required, MaxLength(50)] string CategoryName,decimal DistanceKm,int MaxParticipants,decimal Fee);
public record EnrolmentRequest([Range(1, int.MaxValue)] int CategoryId);
public record ResultRequest([Range(1, int.MaxValue)] int EnrolmentId,TimeSpan FinishTime,int? Position,string? Notes);
public record UpdateResultRequest(TimeSpan FinishTime,int? Position,string? Notes);
public record PaymentRequest([Range(1, int.MaxValue)] int EnrolmentId,[Required, MaxLength(50)] string PaymentMethod,[MaxLength(100)] string? TransactionId);
public record StorageUploadResponse(string Container, string BlobName, string ContentType);
