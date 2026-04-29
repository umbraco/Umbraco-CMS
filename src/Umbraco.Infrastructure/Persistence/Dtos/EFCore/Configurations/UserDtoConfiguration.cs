using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class UserDtoConfiguration : IEntityTypeConfiguration<UserDto>
{
    public void Configure(EntityTypeBuilder<UserDto> builder)
    {
        builder.ToTable(UserDto.TableName);

        builder.HasKey(x => x.Id)
            .HasName("PK_user");

        builder.Property(x => x.Id)
            .HasColumnName(UserDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Disabled)
            .HasColumnName("userDisabled")
            .HasDefaultValue(false);

        builder.Property(x => x.Key)
            .HasColumnName(UserDto.KeyColumnName)
            .IsRequired();

        builder.Property(x => x.NoConsole)
            .HasColumnName("userNoConsole")
            .HasDefaultValue(false);

        builder.Property(x => x.UserName)
            .HasColumnName("userName")
            .IsRequired();

        builder.Property(x => x.Login)
            .HasColumnName("userLogin")
            .HasMaxLength(125);

        builder.Property(x => x.Password)
            .HasColumnName("userPassword")
            .HasMaxLength(500);

        builder.Property(x => x.PasswordConfig)
            .HasColumnName("passwordConfig")
            .HasMaxLength(500);

        builder.Property(x => x.Email)
            .HasColumnName("userEmail")
            .IsRequired();

        builder.Property(x => x.UserLanguage)
            .HasColumnName("userLanguage")
            .HasMaxLength(10);

        builder.Property(x => x.SecurityStampToken)
            .HasColumnName("securityStampToken")
            .HasMaxLength(255);

        builder.Property(x => x.FailedLoginAttempts)
            .HasColumnName("failedLoginAttempts");

        builder.Property(x => x.LastLockoutDate)
            .HasColumnName("lastLockoutDate");

        builder.Property(x => x.LastPasswordChangeDate)
            .HasColumnName("lastPasswordChangeDate");

        builder.Property(x => x.LastLoginDate)
            .HasColumnName("lastLoginDate");

        builder.Property(x => x.EmailConfirmedDate)
            .HasColumnName("emailConfirmedDate");

        builder.Property(x => x.InvitedDate)
            .HasColumnName("invitedDate");

        builder.Property(x => x.CreateDate)
            .HasColumnName("createDate")
            .IsRequired();

        builder.Property(x => x.UpdateDate)
            .HasColumnName("updateDate")
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasDefaultValue<short>(0)
            .IsRequired();

        builder.Property(x => x.Avatar)
            .HasColumnName("avatar")
            .HasMaxLength(500);

        // IX_umbracoUser_userKey (unique non-clustered)
        builder.HasIndex(x => x.Key)
            .IsUnique()
            .HasDatabaseName($"IX_{UserDto.TableName}_userKey");

        // Non-clustered index on userLogin
        builder.HasIndex(x => x.Login);
    }
}
