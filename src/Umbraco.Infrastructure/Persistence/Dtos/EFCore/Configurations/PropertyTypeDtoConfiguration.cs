using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class PropertyTypeDtoConfiguration : IEntityTypeConfiguration<PropertyTypeDto>
{
    public void Configure(EntityTypeBuilder<PropertyTypeDto> builder)
    {
        builder.ToTable(PropertyTypeDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(PropertyTypeDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.DataTypeId)
            .HasColumnName(PropertyTypeDto.DataTypeIdColumnName);

        builder.Property(x => x.ContentTypeId)
            .HasColumnName(PropertyTypeDto.ContentTypeIdColumnName);

        builder.Property(x => x.PropertyTypeGroupId)
            .HasColumnName(PropertyTypeDto.PropertyTypeGroupIdColumnName);

        builder.Property(x => x.Alias)
            .HasColumnName("Alias");

        builder.Property(x => x.Name)
            .HasColumnName("Name");

        builder.Property(x => x.SortOrder)
            .HasColumnName("sortOrder")
            .HasDefaultValue(0);

        builder.Property(x => x.Mandatory)
            .HasColumnName("mandatory")
            .HasDefaultValue(false);

        builder.Property(x => x.MandatoryMessage)
            .HasColumnName("mandatoryMessage")
            .HasMaxLength(500);

        builder.Property(x => x.ValidationRegExp)
            .HasColumnName("validationRegExp");

        builder.Property(x => x.ValidationRegExpMessage)
            .HasColumnName("validationRegExpMessage")
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasColumnName("Description")
            .HasMaxLength(2000);

        builder.Property(x => x.LabelOnTop)
            .HasColumnName("labelOnTop")
            .HasDefaultValue(false);

        builder.Property(x => x.Variations)
            .HasColumnName("variations")
            .HasDefaultValue((byte)1);

        builder.Property(x => x.UniqueId)
            .HasColumnName("UniqueId");

        // IX_cmsPropertyTypeUniqueID
        builder.HasIndex(x => x.UniqueId)
            .IsUnique()
            .HasDatabaseName($"IX_{PropertyTypeDto.TableName}UniqueID");

        // IX_cmsPropertyTypeAlias
        builder.HasIndex(x => x.Alias)
            .HasDatabaseName($"IX_{PropertyTypeDto.TableName}Alias");

        // FK: DataTypeId -> umbracoDataType.nodeId (PK of DataTypeDto)
        builder.HasOne<DataTypeDto>()
            .WithMany()
            .HasForeignKey(x => x.DataTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK: ContentTypeId -> cmsContentType.nodeId (non-PK, unique column on ContentTypeDto)
        // Using Restrict to avoid multiple cascade paths (PropertyTypeGroup also cascades from ContentType).
        builder.HasOne<ContentTypeDto>()
            .WithMany()
            .HasForeignKey(x => x.ContentTypeId)
            .HasPrincipalKey(nameof(ContentTypeDto.NodeId))
            .OnDelete(DeleteBehavior.Restrict);

        // FK: PropertyTypeGroupId -> cmsPropertyTypeGroup.id (optional)
        builder.HasOne<PropertyTypeGroupDto>()
            .WithMany()
            .HasForeignKey(x => x.PropertyTypeGroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
