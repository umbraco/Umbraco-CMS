using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class MemberPropertyTypeDtoConfiguration : IEntityTypeConfiguration<MemberPropertyTypeDto>
{
    public void Configure(EntityTypeBuilder<MemberPropertyTypeDto> builder)
    {
        builder.ToTable(MemberPropertyTypeDto.TableName);

        builder.HasKey(x => x.PrimaryKey);

        builder.Property(x => x.PrimaryKey)
            .HasColumnName(MemberPropertyTypeDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NodeId)
            .HasColumnName("NodeId");

        builder.Property(x => x.PropertyTypeId)
            .HasColumnName(MemberPropertyTypeDto.PropertyTypeIdColumnName);

        builder.Property(x => x.CanEdit)
            .HasColumnName("memberCanEdit")
            .HasDefaultValue(false);

        builder.Property(x => x.ViewOnProfile)
            .HasColumnName("viewOnProfile")
            .HasDefaultValue(false);

        builder.Property(x => x.IsSensitive)
            .HasColumnName("isSensitive")
            .HasDefaultValue(false);

        builder.HasOne(x => x.PropertyTypeDto)
            .WithOne(x => x.MemberPropertyTypeDto)
            .HasForeignKey<MemberPropertyTypeDto>(x => x.PropertyTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName($"FK_{MemberPropertyTypeDto.TableName}_{PropertyTypeDto.TableName}");
    }
}
