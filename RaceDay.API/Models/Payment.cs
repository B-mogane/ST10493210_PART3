using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models;

[Table("Payment")]
public class Payment
{
    [Key] public int PaymentID { get; set; }
    public int EnrolmentID { get; set; }
    [Column(TypeName = "decimal(10,2)")] public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    [Required, MaxLength(50)] public string PaymentMethod { get; set; } = "";
    [MaxLength(100)] public string? TransactionID { get; set; }
    public Enrolment? Enrolment { get; set; }
}
