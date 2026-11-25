using Moq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Stored values and properties for the entity picker value editor tests.
/// </summary>
internal static class EntityPickerTestValues
{
    public static string Udi(string entityType, Guid key) => new GuidUdi(entityType, key).ToString();

    public static string UdiList(string entityType, params Guid[] keys) => string.Join(',', keys.Select(key => Udi(entityType, key)));

    public static IProperty Property(object? value)
    {
        var property = new Mock<IProperty>();
        property
            .Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(value);
        return property.Object;
    }
}
