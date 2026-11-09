using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("Category")]
public class Category
{
    [Key] public int CategoryID { get; set; }
    public int EventID { get; set; }
    [Required, MaxLength(50)] public string CategoryName { get; set; } = "";
    [Column(TypeName = "decimal(5,2)")] public decimal DistanceKm { get; set; }
    public int MaxParticipants { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal Fee { get; set; }
    public Event? Event { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
