using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models;

[Table("sessions")]
public class Sessions
{
    [Key]
    [Required]
    [Column("id", TypeName = "char(56)")]
    public string ID { get; set; }


    [Required]
    [Column("userid")]
    public int UserID { get; set; }

    [Required]
    [Column("expiration")]
    public DateTime Expiration { get; set; }
}