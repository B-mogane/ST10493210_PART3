using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("Result")]
public class Result
{
    [Key] public int ResultID { get; set; }
    public int EnrolmentID { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int? Position { get; set; }
    public string? Notes { get; set; }
    public Enrolment? Enrolment { get; set; }
}
