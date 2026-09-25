using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Sqlite;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.EFCore;

/// <summary>
///     A fixed-scale decimal column hands back trailing zeros the stored value never had. The DTO normalises those
///     away, and that has to hold for values EF Core materialises, not only for values assigned in code.
/// </summary>
[TestFixture]
public class PropertyDataDtoMaterializationTests
{
    [Test]
    public async Task Materialized_Decimal_Value_Is_Normalized()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using UmbracoDbContext context = UmbracoDbContextTestFactory.Create(
            optionsBuilder => optionsBuilder.UseSqlite(connection),
            [new SqliteCollationModelCustomizer(), new SqliteContentVersionDtoModelCustomizer()]);
        await context.Database.EnsureCreatedAsync();

        // Decimals live in a TEXT column on SQLite, so the stored text stands in for the scale a fixed-precision
        // column pads the value out to on other providers.
        await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = OFF");
        await context.Database.ExecuteSqlRawAsync(
            $"INSERT INTO {PropertyDataDto.TableName} (versionId, propertyTypeId, decimalValue) VALUES (1, 1, '12.455200000')");

        PropertyDataDto dto = await context.PropertyData.SingleAsync();

        Assert.That(dto.DecimalValue, Is.Not.Null);
        Assert.That(dto.DecimalValue!.Value.ToString(CultureInfo.InvariantCulture), Is.EqualTo("12.4552"));
    }
}
