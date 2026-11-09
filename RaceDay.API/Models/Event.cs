using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RaceDay.API.Models;
[Table("Event")]
public class Event
{
    [Key] public int EventID { get; set; }
    public int OrganiserID { get; set; }
    [Required, MaxLength(100)] public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    [Required, MaxLength(200)] public string Location { get; set; } = "";
    public string? RouteInfo { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal EntryFee { get; set; }
    [Required, MaxLength(20)] public string Status { get; set; } = "Draft";
    [Column(TypeName = "decimal(6,2)")] public decimal DistanceKm { get; set; }
    [Required, MaxLength(20)] public string EventType { get; set; } = "Run";
    [MaxLength(180)] public string? BannerImageName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? Organiser { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}
