using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

internal class WebhookDtoConfiguration : IEntityTypeConfiguration<WebhookDto>
{
    public void Configure(EntityTypeBuilder<WebhookDto> builder)
    {
        builder.ToTable(WebhookDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Key)
            .HasColumnName("key")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Url)
            .HasColumnName("url")
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(x => x.Enabled)
            .HasColumnName("enabled");

        builder
            .HasMany(x => x.Webhook2ContentTypeKeys)
            .WithOne(x => x.Webhook)
            .HasForeignKey(x => x.WebhookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Webhook2Events)
            .WithOne(x => x.Webhook)
            .HasForeignKey(x => x.WebhookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(x => x.Webhook2Headers)
            .WithOne(x => x.Webhook)
            .HasForeignKey(x => x.WebhookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
