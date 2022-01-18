using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models;

public class Character
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public short UserId { get; set; }
    
    [DefaultValue(0)]
    public short Slot { get; set; }
    
    [Required]
    public short ServerId { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [DefaultValue(2)]
    public short State { get; set; }
    
    [DefaultValue(false)]
    public bool IsLegacy { get; set; }
    
    [DefaultValue(false)]
    public bool DoRename { get; set; }
    
    [Required]
    public int CurrentZoneId { get; set; }

    [DefaultValue(1)]
    public byte Guardian { get; set; }
    
    [DefaultValue(1)]
    public byte BirthMonth { get; set; }
    
    [DefaultValue(1)]
    public byte BirthDay { get; set; }

    [NotMapped]
    public int CurrentClass = 3;
    
    [NotMapped]
    public int CurrentJob = 0;
    
    [NotMapped]
    public int CurrentLevel = 1;
    
    [Required]
    public byte InitialTown { get; set; }
    
    [DefaultValue(1)]
    public byte Tribe { get; set; }
    
    public static void Setup(EntityTypeBuilder<Character> builder)
    {
        builder.Property(x => x.Slot)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Name)
            .HasMaxLength(32);
        
        builder.Property(x => x.State)
            .HasDefaultValue(2);
        
        builder.Property(x => x.IsLegacy)
            .HasDefaultValue(false);
        
        builder.Property(x => x.DoRename)
            .HasDefaultValue(false);
        
        builder.Property(x => x.BirthMonth)
            .HasDefaultValue(1);
        
        builder.Property(x => x.BirthDay)
            .HasDefaultValue(1);
        
        builder.Property(x => x.Tribe)
            .HasDefaultValue(1);
        
       
        
    }
}