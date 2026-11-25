using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
internal sealed class MultipleMemberPickerValueConverterTests
{
    private static readonly Guid _first = new("0f4f9a0e-6b1e-4d0f-9b1c-2a3f6e5d4c3b");
    private static readonly Guid _second = new("8c2d7e61-3a4b-4c5d-8e9f-0a1b2c3d4e5f");

    [Test]
    public void Convert_Stored_Selection_To_Members()
        => Assert.That(Convert($"[\"{_second}\",\"{_first}\"]"), Is.EqualTo(new[] { _second, _first }));

    [Test]
    public void Convert_Stored_Udi_List_To_Members()
        => Assert.That(
            Convert($"{new GuidUdi(Constants.UdiEntityType.Member, _first)},{new GuidUdi(Constants.UdiEntityType.Member, _second)}"),
            Is.EqualTo(new[] { _first, _second }));

    private static Guid[]? Convert(string inter)
    {
        var memberService = new Mock<IMemberService>();
        var memberCache = new Mock<IPublishedMemberCache>();
        foreach (Guid key in new[] { _first, _second })
        {
            IMember member = Mock.Of<IMember>(m => m.Key == key);
            memberService.Setup(s => s.GetById(key)).Returns(member);
            memberCache.Setup(c => c.Get(member)).Returns(Mock.Of<IPublishedMember>(m => m.Key == key));
        }

        var converter = new MultipleMemberPickerValueConverter(
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            memberService.Object,
            memberCache.Object);

        var result = converter.ConvertIntermediateToObject(
            Mock.Of<IPublishedElement>(),
            Mock.Of<IPublishedPropertyType>(),
            PropertyCacheLevel.Element,
            inter,
            false) as IEnumerable<IPublishedContent>;

        return result?.Select(x => x.Key).ToArray();
    }
}
