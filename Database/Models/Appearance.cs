using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Database.Models;

public class Appearance
{
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Required]
    public int CharacterId { get; set; }
    
    
    [DefaultValue(0)]
    public byte Size { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte Voice { get; set; } = 0;
    
    [DefaultValue(0)]
    public short SkinColor { get; set; } = 0;
    
    [DefaultValue(0)]
    public short HairStyle { get; set; } = 0;
    
    [DefaultValue(0)]
    public short HairColor { get; set; } = 0;
    
    [DefaultValue(0)]
    public short HairHighlightColor { get; set; } = 0;
    
    [DefaultValue(0)]
    public short HairVariation { get; set; } = 0;
    
    [DefaultValue(0)]
    public short EyeColor { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte CharacteristicsColor { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte FaceType { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte FaceEyebrows { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte FaceEyeShape { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte FaceIrisSize { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte FaceNose { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte FaceMouth { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte FaceFeatures { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte Characteristics { get; set; } = 0;
    
    [DefaultValue(0)]
    public byte Ears { get; set; } = 0;
    
    [DefaultValue(0)]
    public int MainHand { get; set; } = 0;
    
    [DefaultValue(0)]
    public int OffHand { get; set; } = 0;

    [DefaultValue(0)]
    public int Head { get; set; } = 0;
    
    [DefaultValue(0)]
    public int Body { get; set; } = 0;
    
    [DefaultValue(0)]
    public int Legs { get; set; } = 0;
    
    [DefaultValue(0)]
    public int Hands { get; set; } = 0;
    
    [DefaultValue(0)]
    public int Feet { get; set; } = 0;
    
    [DefaultValue(0)]
    public int Waist { get; set; } = 0;
    
    [DefaultValue(0)]
    public int Neck { get; set; } = 0;
    
    [DefaultValue(0)]
    public int RightEar { get; set; } = 0;
    
    [DefaultValue(0)]
    public int LeftEar { get; set; } = 0;
    
    [DefaultValue(0)]
    public int RightIndex { get; set; } = 0;
    
    [DefaultValue(0)]
    public int LeftIndex { get; set; } = 0;
    
    [DefaultValue(0)]
    public int RightFinger { get; set; } = 0;
    
    [DefaultValue(0)]
    public int LeftFinger { get; set; } = 0;
    
    public static void Setup(EntityTypeBuilder<Appearance> builder)
    {
        builder.Property(x => x.Size)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Voice)
            .HasDefaultValue(0);
        
        builder.Property(x => x.SkinColor)
            .HasDefaultValue(0);
        
        builder.Property(x => x.HairStyle)
            .HasDefaultValue(0);
        
        builder.Property(x => x.HairColor)
            .HasDefaultValue(0);
        
        builder.Property(x => x.HairHighlightColor)
            .HasDefaultValue(0);
        
        builder.Property(x => x.HairVariation)
            .HasDefaultValue(0);
        
        builder.Property(x => x.EyeColor)
            .HasDefaultValue(0);
        
        builder.Property(x => x.CharacteristicsColor)
            .HasDefaultValue(0);
        
        builder.Property(x => x.FaceType)
            .HasDefaultValue(0);
        
        builder.Property(x => x.FaceEyebrows)
            .HasDefaultValue(0);
        
        builder.Property(x => x.FaceEyeShape)
            .HasDefaultValue(0);
        
        builder.Property(x => x.FaceIrisSize)
            .HasDefaultValue(0);
        
        builder.Property(x => x.FaceNose)
            .HasDefaultValue(0);
        
        builder.Property(x => x.FaceMouth)
            .HasDefaultValue(0);
        
        builder.Property(x => x.FaceFeatures)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Characteristics)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Ears)
            .HasDefaultValue(0);
        
        builder.Property(x => x.MainHand)
            .HasDefaultValue(0);
        
        builder.Property(x => x.OffHand)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Head)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Body)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Legs)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Hands)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Feet)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Waist)
            .HasDefaultValue(0);
        
        builder.Property(x => x.Neck)
            .HasDefaultValue(0);
        
        builder.Property(x => x.RightEar)
            .HasDefaultValue(0);
        
        builder.Property(x => x.LeftEar)
            .HasDefaultValue(0);
        
        builder.Property(x => x.RightIndex)
            .HasDefaultValue(0);
        
        builder.Property(x => x.LeftIndex)
            .HasDefaultValue(0);
        
        builder.Property(x => x.RightFinger)
            .HasDefaultValue(0);
        
        builder.Property(x => x.LeftFinger)
            .HasDefaultValue(0);
        
        
        
        
    }
}