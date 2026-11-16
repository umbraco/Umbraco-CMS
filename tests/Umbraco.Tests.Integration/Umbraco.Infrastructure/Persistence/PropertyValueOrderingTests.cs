// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

/// <summary>
///     Ordering content by a numeric property follows the numeric order of its values.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class PropertyValueOrderingTests : UmbracoIntegrationTest
{
    private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

    private IContentService ContentService => GetRequiredService<IContentService>();

    private ITemplateService TemplateService => GetRequiredService<ITemplateService>();

    [Test]
    public async Task Ordering_Content_By_An_Integer_Property_Follows_Numeric_Order()
    {
        int[] values = [3, -5, 123456789, 123456780, -6, 0, int.MaxValue, int.MinValue, -40];

        IEnumerable<string> names = await GetNamesOrderedByProperty(Constants.DataTypes.LabelInt, ValueStorageType.Integer, values.Cast<object>());

        Assert.That(names, Is.EqualTo(values.Order().Select(x => NameForValue(x))));
    }

    [Test]
    public async Task Ordering_Content_By_A_Decimal_Property_Follows_Numeric_Order()
    {
        // The pairs differing only in their last digit catch conversions through floating point. They stay within
        // the digits every database's decimal column keeps. Fractions of different lengths and whole numbers catch
        // conversions that pad text: 1.5 must sort above 1.25, and a value without a decimal point keeps its digits.
        decimal[] values =
        [
            2.25m, -1.5m, 10.1m, 0m, -0.25m, 99999999999.5m, -12345.678m, -0.000001m,
            99999999998.999999m, 99999999998.999998m, -99999999998.999999m, -99999999998.999998m,
            1.5m, 1.25m, -1.25m, -5m, 7m,
        ];

        IEnumerable<string> names = await GetNamesOrderedByProperty(Constants.DataTypes.LabelDecimal, ValueStorageType.Decimal, values.Cast<object>());

        Assert.That(names, Is.EqualTo(values.Order().Select(x => NameForValue(x))));
    }

    private async Task<IEnumerable<string>> GetNamesOrderedByProperty(int dataTypeId, ValueStorageType storageType, IEnumerable<object> values)
    {
        ITemplate template = TemplateBuilder.CreateTextPageTemplate();
        await TemplateService.CreateAsync(template, Constants.Security.SuperUserKey);

        ContentTypeBuilder builder = ContentTypeBuilder.CreateSimpleContentTypeHelper("sortPage", "Sort Page", defaultTemplateId: template.Id);
        builder.AddPropertyType()
            .WithAlias("sortValue")
            .WithName("Sort Value")
            .WithDataTypeId(dataTypeId)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Label)
            .WithValueStorageType(storageType)
            .Done();
        var contentType = (ContentType)builder.Build();
        contentType.AllowedAsRoot = true;
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        foreach (var value in values)
        {
            Content content = ContentBuilder.CreateBasicContent(contentType);
            content.Name = NameForValue(value);
            content.SetValue("sortValue", value);
            ContentService.Save(content);
        }

        return ContentService
            .GetPagedChildren(Constants.System.Root, 0, 100, out _, null, null, Ordering.By("sortValue", isCustomField: true))
            .Select(x => x.Name!);
    }

    // Spells out the sign: siblings must have distinct URL segments, and "-1.5" and "1.5" share one.
    private static string NameForValue(object value)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture)!;
        return text.StartsWith('-') ? "minus " + text[1..] : text;
    }
}
