using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class UserStartNodeDtoConfiguration : IEntityTypeConfiguration<UserStartNodeDto>
{
    public void Configure(EntityTypeBuilder<UserStartNodeDto> builder)
    {
        builder.ToTable(UserStartNodeDto.TableName);

        builder.HasKey(x => x.Id)
            .HasName("PK_userStartNode");

        builder.Property(x => x.Id)
            .HasColumnName(UserStartNodeDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("userId");

        builder.Property(x => x.StartNode)
            .HasColumnName("startNode");

        builder.Property(x => x.StartNodeType)
            .HasColumnName("startNodeType");

        // IX_umbracoUserStartNode_startNodeType
        builder.HasIndex(x => new { x.StartNodeType, x.StartNode, x.UserId })
            .IsUnique()
            .HasDatabaseName($"IX_{UserStartNodeDto.TableName}_startNodeType");

        builder.HasOne<UserDto>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName($"FK_{UserStartNodeDto.TableName}_umbracoUser_id")
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.StartNode)
            .HasConstraintName($"FK_{UserStartNodeDto.TableName}_umbracoNode_id")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
