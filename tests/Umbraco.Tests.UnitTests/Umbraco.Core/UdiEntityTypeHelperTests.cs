// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core;

[TestFixture]
public class UdiEntityTypeHelperTests
{
    // UmbracoObjectTypes values that intentionally have no UDI mapping. Add to this list
    // (with a reason) only if a new value genuinely should not round-trip via UdiEntityTypeHelper.
    private static readonly UmbracoObjectTypes[] _unmappedUmbracoObjectTypes =
    {
        UmbracoObjectTypes.Unknown,            // sentinel/default
        UmbracoObjectTypes.ROOT,               // system root, not a UDI-addressable entity
        UmbracoObjectTypes.RecycleBin,         // system folder, not a UDI-addressable entity
        UmbracoObjectTypes.ElementRecycleBin,  // system folder, not a UDI-addressable entity
        UmbracoObjectTypes.IdReservation,      // identifier reservation marker
    };

    [TestCaseSource(nameof(MappedUmbracoObjectTypes))]
    public void FromUmbracoObjectType_RoundTripsThroughToUmbracoObjectType(UmbracoObjectTypes value, string udiType)
        => Assert.That(UdiEntityTypeHelper.ToUmbracoObjectType(udiType), Is.EqualTo(value));

    [TestCaseSource(nameof(MappedUdiEntityTypes))]
    public void ToUmbracoObjectType_RoundTripsThroughFromUmbracoObjectType(string udiType, UmbracoObjectTypes value)
        => Assert.That(UdiEntityTypeHelper.FromUmbracoObjectType(value), Is.EqualTo(udiType));

    [TestCaseSource(nameof(AllUmbracoObjectTypes))]
    public void UmbracoObjectType_HasUdiMappingOrIsExplicitlyExcluded(UmbracoObjectTypes value)
    {
        if (_unmappedUmbracoObjectTypes.Contains(value))
        {
            Assert.Throws<NotSupportedException>(
                () => UdiEntityTypeHelper.FromUmbracoObjectType(value),
                $"{value} is listed in {nameof(_unmappedUmbracoObjectTypes)} but FromUmbracoObjectType returned a value - remove it from the exclusion list.");
            return;
        }

        Assert.DoesNotThrow(
            () => UdiEntityTypeHelper.FromUmbracoObjectType(value),
            $"{value} has no UDI mapping. Either wire it up in {nameof(UdiEntityTypeHelper)}, or add it to {nameof(_unmappedUmbracoObjectTypes)} with a reason.");
    }

    private static IEnumerable<TestCaseData> AllUmbracoObjectTypes() =>
        Enum.GetValues<UmbracoObjectTypes>()
            .Select(value => new TestCaseData(value).SetName($"{nameof(UmbracoObjectTypes)}.{value}"));

    private static IEnumerable<TestCaseData> MappedUmbracoObjectTypes()
    {
        foreach (UmbracoObjectTypes value in Enum.GetValues<UmbracoObjectTypes>())
        {
            string udiType;
            try
            {
                udiType = UdiEntityTypeHelper.FromUmbracoObjectType(value);
            }
            catch (NotSupportedException)
            {
                continue;
            }

            yield return new TestCaseData(value, udiType).SetName($"{nameof(UmbracoObjectTypes)}.{value}");
        }
    }

    private static IEnumerable<TestCaseData> MappedUdiEntityTypes()
    {
        foreach (FieldInfo field in typeof(Constants.UdiEntityType)
            .GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.IsLiteral is false || field.FieldType != typeof(string))
            {
                continue;
            }

            string udiType = (string)field.GetRawConstantValue()!;

            UmbracoObjectTypes value;
            try
            {
                value = UdiEntityTypeHelper.ToUmbracoObjectType(udiType);
            }
            catch (NotSupportedException)
            {
                continue;
            }

            yield return new TestCaseData(udiType, value).SetName($"{nameof(Constants.UdiEntityType)}.{field.Name}");
        }
    }
}
