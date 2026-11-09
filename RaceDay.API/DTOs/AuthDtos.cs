using System.ComponentModel.DataAnnotations;
namespace RaceDay.API.DTOs;
public record RegisterRequest([Required, MaxLength(50)] string FirstName,[Required, MaxLength(50)] string LastName,[Required, EmailAddress, MaxLength(100)] string Email,[Required, MinLength(8)] string Password,[Required] string Role);
public record LoginRequest([Required, EmailAddress] string Email,[Required] string Password);
public record UserResponse(int UserID, string FirstName, string LastName, string Email, string Role, string? ProfilePictureName = null);
public record AuthResponse(string Token, UserResponse User);
public record UpdateProfileRequest([Required, MaxLength(50)] string FirstName,[Required, MaxLength(50)] string LastName,string? Password,string? ProfilePictureName = null);
