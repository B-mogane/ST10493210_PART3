using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("Enrolment")]
public class Enrolment
{
    [Key] public int EnrolmentID { get; set; }
    public int UserID { get; set; }
    public int CategoryID { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    [Required, MaxLength(20)] public string Status { get; set; } = "Confirmed";
    public User? User { get; set; }
    public Category? Category { get; set; }
    public Result? Result { get; set; }
    public Payment? Payment { get; set; }
}
