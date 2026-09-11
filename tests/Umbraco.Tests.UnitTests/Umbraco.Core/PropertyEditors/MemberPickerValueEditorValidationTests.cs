using System.ComponentModel.DataAnnotations;
using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Asserts the member type filter and item count rules the member picker editors share, which is what makes moving
/// off the multi node tree picker possible.
/// </summary>
[TestFixture]
internal sealed class MemberPickerValueEditorValidationTests
{
    [Test]
    public void A_Single_Member_Picker_Allows_A_Member_Of_An_Allowed_Type()
    {
        var allowedTypeKey = Guid.NewGuid();
        var memberKey = Guid.NewGuid();
        var valueEditor = CreateSingleValueEditor(MemberOfType(memberKey, allowedTypeKey));
        valueEditor.ConfigurationObject = new MemberPickerConfiguration { Filter = allowedTypeKey.ToString() };

        ValidateResult(true, valueEditor.Validate(memberKey.ToString(), false, null, PropertyValidationContext.Empty()));
    }

    [Test]
    public void A_Single_Member_Picker_Rejects_A_Member_Of_Another_Type()
    {
        var memberKey = Guid.NewGuid();
        var valueEditor = CreateSingleValueEditor(MemberOfType(memberKey, Guid.NewGuid()));
        valueEditor.ConfigurationObject = new MemberPickerConfiguration { Filter = Guid.NewGuid().ToString() };

        ValidateResult(false, valueEditor.Validate(memberKey.ToString(), false, null, PropertyValidationContext.Empty()));
    }

    [Test]
    public void A_Single_Member_Picker_Allows_Any_Member_When_No_Filter_Is_Configured()
    {
        var memberKey = Guid.NewGuid();
        var valueEditor = CreateSingleValueEditor(MemberOfType(memberKey, Guid.NewGuid()));
        valueEditor.ConfigurationObject = new MemberPickerConfiguration();

        ValidateResult(true, valueEditor.Validate(memberKey.ToString(), false, null, PropertyValidationContext.Empty()));
    }

    [Test]
    public void A_Multiple_Member_Picker_Rejects_A_Member_Of_Another_Type()
    {
        var allowedTypeKey = Guid.NewGuid();
        var allowedMemberKey = Guid.NewGuid();
        var otherMemberKey = Guid.NewGuid();
        var valueEditor = CreateMultipleValueEditor(
            MemberOfType(allowedMemberKey, allowedTypeKey),
            MemberOfType(otherMemberKey, Guid.NewGuid()));
        valueEditor.ConfigurationObject = new MultipleMemberPickerConfiguration { Filter = allowedTypeKey.ToString() };

        IEnumerable<ValidationResult> result = valueEditor.Validate(
            new List<string> { allowedMemberKey.ToString(), otherMemberKey.ToString() },
            false,
            null,
            PropertyValidationContext.Empty());

        ValidateResult(false, result);
    }

    [Test]
    public void A_Multiple_Member_Picker_Reports_A_Member_That_No_Longer_Exists()
    {
        var valueEditor = CreateMultipleValueEditor();
        valueEditor.ConfigurationObject = new MultipleMemberPickerConfiguration { Filter = Guid.NewGuid().ToString() };

        IEnumerable<ValidationResult> result = valueEditor.Validate(
            new List<string> { Guid.NewGuid().ToString() },
            false,
            null,
            PropertyValidationContext.Empty());

        ValidateResult(false, result);
    }

    // A configured minimum is a floor on a selection that is in use, not a way of making the property required -
    // see #23486 - so an empty selection never fails it.
    [TestCase(0, 1, true)]
    [TestCase(1, 1, true)]
    [TestCase(1, 2, false)]
    [TestCase(0, 2, true)]
    public void A_Multiple_Member_Picker_Validates_The_Minimum_Number_Of_Members(int memberCount, int min, bool succeed)
    {
        var valueEditor = CreateMultipleValueEditor();
        valueEditor.ConfigurationObject = new MultipleMemberPickerConfiguration
        {
            ValidationLimit = new MultipleMemberPickerConfiguration.NumberRange { Min = min },
        };

        ValidateResult(succeed, valueEditor.Validate(Value(memberCount), false, null, PropertyValidationContext.Empty()));
    }

    [TestCase(1, 2, true)]
    [TestCase(2, 2, true)]
    [TestCase(3, 2, false)]
    public void A_Multiple_Member_Picker_Validates_The_Maximum_Number_Of_Members(int memberCount, int max, bool succeed)
    {
        var valueEditor = CreateMultipleValueEditor();
        valueEditor.ConfigurationObject = new MultipleMemberPickerConfiguration
        {
            ValidationLimit = new MultipleMemberPickerConfiguration.NumberRange { Max = max },
        };

        ValidateResult(succeed, valueEditor.Validate(Value(memberCount), false, null, PropertyValidationContext.Empty()));
    }

    private static List<string> Value(int memberCount)
        => Enumerable.Range(0, memberCount).Select(_ => Guid.NewGuid().ToString()).ToList();

    private static IMember MemberOfType(Guid memberKey, Guid memberTypeKey)
    {
        var memberType = new Mock<ISimpleContentType>();
        memberType.SetupGet(x => x.Key).Returns(memberTypeKey);

        var member = new Mock<IMember>();
        member.SetupGet(x => x.Key).Returns(memberKey);
        member.SetupGet(x => x.ContentType).Returns(memberType.Object);

        return member.Object;
    }

    private static void ValidateResult(bool succeed, IEnumerable<ValidationResult> result)
    {
        if (succeed)
        {
            Assert.IsEmpty(result);
        }
        else
        {
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }

    private static MemberPickerPropertyEditor.MemberPickerPropertyValueEditor CreateSingleValueEditor(
        params IMember[] members)
    {
        (Mock<IMemberService> memberService, Mock<ICoreScopeProvider> coreScopeProvider) = Dependencies(members);

        return new MemberPickerPropertyEditor.MemberPickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            memberService.Object,
            Mock.Of<ILocalizedTextService>(),
            coreScopeProvider.Object)
        {
            ConfigurationObject = new MemberPickerConfiguration(),
        };
    }

    private static MultipleMemberPickerPropertyEditor.MultipleMemberPickerPropertyValueEditor CreateMultipleValueEditor(
        params IMember[] members)
    {
        (Mock<IMemberService> memberService, Mock<ICoreScopeProvider> coreScopeProvider) = Dependencies(members);

        return new MultipleMemberPickerPropertyEditor.MultipleMemberPickerPropertyValueEditor(
            Mock.Of<IShortStringHelper>(),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            Mock.Of<IIOHelper>(),
            new DataEditorAttribute("alias"),
            Mock.Of<ILocalizedTextService>(),
            memberService.Object,
            coreScopeProvider.Object)
        {
            ConfigurationObject = new MultipleMemberPickerConfiguration(),
        };
    }

    private static (Mock<IMemberService> MemberService, Mock<ICoreScopeProvider> CoreScopeProvider) Dependencies(
        IMember[] members)
    {
        var memberService = new Mock<IMemberService>();
        foreach (IMember member in members)
        {
            memberService.Setup(x => x.GetById(member.Key)).Returns(member);
        }

        var coreScopeProvider = new Mock<ICoreScopeProvider>();
        coreScopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());

        return (memberService, coreScopeProvider);
    }
}
