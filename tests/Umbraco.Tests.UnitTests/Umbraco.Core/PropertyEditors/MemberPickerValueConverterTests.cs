using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
internal sealed class MemberPickerValueConverterTests
{
    [Test]
    public void Convert_Stored_Udi_List_To_First_Member()
    {
        var first = Guid.NewGuid();
        var converter = new MemberPickerValueConverter(
            Mock.Of<IMemberService>(),
            Mock.Of<IPublishedMemberCache>(),
            Mock.Of<IExternalMemberService>());

        var result = converter.ConvertSourceToIntermediate(
            Mock.Of<IPublishedElement>(),
            Mock.Of<IPublishedPropertyType>(),
            $"{new GuidUdi(Constants.UdiEntityType.Member, first)},{new GuidUdi(Constants.UdiEntityType.Member, Guid.NewGuid())}",
            false);

        Assert.AreEqual(new GuidUdi(Constants.UdiEntityType.Member, first), result);
    }

    [Test]
    public void Convert_Stored_Json_Array_Of_Keys_To_First_Member()
    {
        var first = Guid.NewGuid();
        var converter = new MemberPickerValueConverter(
            Mock.Of<IMemberService>(),
            Mock.Of<IPublishedMemberCache>(),
            Mock.Of<IExternalMemberService>());

        var result = converter.ConvertSourceToIntermediate(
            Mock.Of<IPublishedElement>(),
            Mock.Of<IPublishedPropertyType>(),
            $"[\"{first}\",\"{Guid.NewGuid()}\"]",
            false);

        Assert.AreEqual(new GuidUdi(Constants.UdiEntityType.Member, first), result);
    }
}
