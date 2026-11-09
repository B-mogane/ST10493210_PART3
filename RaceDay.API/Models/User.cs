using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models;
[Table("User")]
public class User
{
    [Key] public int UserID { get; set; }
    [Required, MaxLength(50)] public string FirstName { get; set; } = "";
    [Required, MaxLength(50)] public string LastName { get; set; } = "";
    [Required, MaxLength(100), EmailAddress] public string Email { get; set; } = "";
    [Required, MaxLength(255)] public string PasswordHash { get; set; } = "";
    [Required, MaxLength(20)] public string Role { get; set; } = "Participant";
    [MaxLength(180)] public string? ProfilePictureName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
