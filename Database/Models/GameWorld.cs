using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models;

public class GameWorld
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] public string Name { get; set; }

    [Required] public string Address { get; set; }

    [Required] public int Port { get; set; }

    [DefaultValue(1)]
    public short Position { get; set; } = 1;

    [DefaultValue(true)]
    public bool IsActive { get; set; } = true;

    [DefaultValue(5000)]
    public int MaxAllowChars { get; set; } = 5000;

    [DefaultValue(0)]
    public int CurrentOnlineChars { get; set; } = 0;

    [NotMapped] public int Population => CurrentOnlineChars < 1000 ? 1 : MaxAllowChars / CurrentOnlineChars;

    public static void Setup(EntityTypeBuilder<GameWorld> builder)
    {
        builder.Property(x => x.MaxAllowChars)
            .HasDefaultValue(5000);

        builder.Property(x => x.CurrentOnlineChars)
            .HasDefaultValue(0);

        builder.Property(x => x.Position)
            .HasDefaultValue(1);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);
    }
}