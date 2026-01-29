// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_17_3_0;
using Umbraco.Cms.Infrastructure.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V17_2_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PopulateSortableValueForDatePropertyDataTest : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentEditingService ContentEditingService => GetRequiredService<IContentEditingService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    private IConfigurationEditorJsonSerializer ConfigurationEditorJsonSerializer
        => GetRequiredService<IConfigurationEditorJsonSerializer>();

    [Test]
    public async Task Can_Populate_SortableValue_For_Existing_Date_Property_Data()
    {
        // Arrange: Create a content type with a DateTimeWithTimeZone property and content with date values.
        var (contentType, contentItems) = await PrepareTestData();

        // Verify initial state: sortableValue should be populated after content creation.
        await AssertSortableValuesPopulated(contentType.Id, contentItems.Length);

        // Clear sortableValue to simulate pre-migration state.
        await ClearSortableValues(contentType.Id);

        // Verify sortableValue is now NULL.
        await AssertSortableValuesCleared(contentType.Id);

        // Act: Run the migration.
        var rowsAffected = await ExecuteMigration();

        // Assert: Verify sortableValue is populated again with correct values.
        Assert.AreEqual(contentItems.Length, rowsAffected);
        await AssertSortableValuesPopulated(contentType.Id, contentItems.Length);
        await AssertSortableValuesAreCorrectlyFormatted(contentType.Id);
    }

    [Test]
    public async Task Migration_Does_Not_Update_Already_Populated_SortableValues()
    {
        // Arrange: Create content with date values.
        var (contentType, _) = await PrepareTestData();

        // Verify initial state.
        await AssertSortableValuesPopulated(contentType.Id, 3);

        // Act: Run the migration (should not update existing values).
        var rowsAffected = await ExecuteMigration();

        // Assert: No rows should be affected since sortableValue is already populated.
        Assert.AreEqual(0, rowsAffected);
    }

    private async Task<(IContentType ContentType, IContent[] ContentItems)> PrepareTestData()
    {
        var contentType = await CreateContentTypeWithDateProperty();

        // Create content items with different dates.
        var dates = new[]
        {
            new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.Zero),
            new DateTimeOffset(2024, 6, 20, 14, 45, 0, TimeSpan.FromHours(2)),
            new DateTimeOffset(2024, 12, 25, 8, 0, 0, TimeSpan.FromHours(-5)),
        };

        var contentItems = new IContent[dates.Length];
        for (var i = 0; i < dates.Length; i++)
        {
            var dateValue = new DateTimeEditorValue
            {
                Date = dates[i].ToString("O"),
                TimeZone = "UTC",
            };

            var createModel = new ContentCreateModel
            {
                Key = Guid.NewGuid(),
                ContentTypeKey = contentType.Key,
                ParentKey = Constants.System.RootKey,
                Variants = [new VariantModel { Name = $"Event {i + 1}" }],
                Properties =
                [
                    new PropertyValueModel
                    {
                        Alias = "eventDate",
                        Value = JsonSerializer.Serialize(dateValue),
                    },
                ],
            };

            var result = await ContentEditingService.CreateAsync(createModel, Constants.Security.SuperUserKey);
            Assert.IsTrue(result.Success, $"Failed to create content: {result.Status}");
            contentItems[i] = result.Result.Content!;
        }

        return (contentType, contentItems);
    }

    private async Task<IContentType> CreateContentTypeWithDateProperty()
    {
        // Get the DateTimeWithTimeZone property editor.
        var propertyEditor = PropertyEditors.First(e => e.Alias == Constants.PropertyEditors.Aliases.DateTimeWithTimeZone);

        // Create a data type for the date property.
        var dataType = new DataType(propertyEditor, ConfigurationEditorJsonSerializer)
        {
            Name = "Test DateTime",
            DatabaseType = ValueStorageType.Ntext,
        };
        await DataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);

        // Create a content type with the date property.
        var contentType = new ContentTypeBuilder()
            .WithAlias("eventPage")
            .WithName("Event Page")
            .WithIsElement(false)
            .AddPropertyType()
                .WithAlias("eventDate")
                .WithName("Event Date")
                .WithDataTypeId(dataType.Id)
                .Done()
            .Build();
        contentType.AllowedAsRoot = true;

        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);
        return (await ContentTypeService.GetAsync(contentType.Key))!;
    }

    private async Task<List<PropertyDataDto>> GetPropertyDataWithSortableValues(int contentTypeId)
    {
        using IScope scope = ScopeProvider.CreateScope();
        Sql<ISqlContext> sql = scope.Database.SqlContext.Sql()
            .Select<PropertyDataDto>()
            .From<PropertyDataDto>()
            .InnerJoin<PropertyTypeDto>().On<PropertyDataDto, PropertyTypeDto>(pd => pd.PropertyTypeId, pt => pt.Id)
            .Where<PropertyTypeDto>(pt => pt.ContentTypeId == contentTypeId)
            .Where<PropertyDataDto>(pd => pd.SortableValue != null);

        var results = await scope.Database.FetchAsync<PropertyDataDto>(sql);
        scope.Complete();
        return results;
    }

    private async Task AssertSortableValuesPopulated(int contentTypeId, int expectedCount)
    {
        var results = await GetPropertyDataWithSortableValues(contentTypeId);
        Assert.AreEqual(expectedCount, results.Count, "Expected sortableValue to be populated for all property data rows.");
    }

    private async Task AssertSortableValuesCleared(int contentTypeId)
    {
        var results = await GetPropertyDataWithSortableValues(contentTypeId);
        Assert.AreEqual(0, results.Count, "Expected sortableValue to be NULL for all property data rows.");
    }

    private async Task AssertSortableValuesAreCorrectlyFormatted(int contentTypeId)
    {
        var results = await GetPropertyDataWithSortableValues(contentTypeId);

        foreach (var dto in results)
        {
            // Verify the sortable value is in a valid ISO 8601 format.
            Assert.IsNotNull(dto.SortableValue);
            Assert.IsTrue(
                DateTimeOffset.TryParse(dto.SortableValue, out _),
                $"SortableValue '{dto.SortableValue}' is not a valid date format.");

            // Verify it's normalized to UTC (ends with Z or +00:00).
            Assert.IsTrue(
                dto.SortableValue.EndsWith("Z") || dto.SortableValue.EndsWith("+00:00"),
                $"SortableValue '{dto.SortableValue}' is not normalized to UTC.");
        }
    }

    private async Task ClearSortableValues(int contentTypeId)
    {
        using IScope scope = ScopeProvider.CreateScope();
        var sql = $@"
UPDATE umbracoPropertyData
SET sortableValue = NULL
WHERE propertyTypeId IN (
    SELECT id
    FROM cmsPropertyType
    WHERE contentTypeId = {contentTypeId}
)";
        await scope.Database.ExecuteAsync(sql);
        scope.Complete();
    }

    private Task<int> ExecuteMigration()
    {
        using IScope scope = ScopeProvider.CreateScope();
        var rowsAffected = PopulateSortableValueForDatePropertyData.ExecuteMigration(
            scope.Database,
            (DatabaseType)scope.Database.DatabaseType,
            NullLogger<PopulateSortableValueForDatePropertyData>.Instance);
        scope.Complete();
        return Task.FromResult(rowsAffected);
    }
}
