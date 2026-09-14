// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

[TestFixture]
public class UserGroupTests
{
    private static int _swedishLanguageId = 1;
    private static int _danishLanguageId = 2;
    private static int _germanLanguageId = 3 ;

    [SetUp]
    public void SetUp() => _builder = new UserGroupBuilder();

    private UserGroupBuilder _builder;

    [Test]
    public void Can_Deep_Clone()
    {
        var item = Build();

        var clone = (IUserGroup)item.DeepClone();

        var x = clone.Equals(item);
        Assert.That(item, Is.Not.SameAs(clone));
        Assert.That(item, Is.EqualTo(clone));

        Assert.That(item.AllowedSections.Count(), Is.EqualTo(clone.AllowedSections.Count()));
        Assert.That(item.AllowedSections, Is.Not.SameAs(clone.AllowedSections));

        // Verify normal properties with reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(item, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var item = Build();

        var json = JsonSerializer.Serialize(item);
        Debug.Print(json);
    }

    [Test]
    public void CanCheckIfUserHasAccessToLanguage()
    {
        var userGrp = new UserGroup(new MockShortStringHelper());
        userGrp.AddAllowedLanguage(_swedishLanguageId);
        userGrp.AddAllowedLanguage(_danishLanguageId);

        IReadOnlyUserGroup grp = userGrp;

        Assert.Multiple(() =>
        {
            Assert.That(grp.HasAccessToLanguage(_swedishLanguageId), Is.True);
            Assert.That(grp.HasAccessToLanguage(_danishLanguageId), Is.True);
            Assert.That(grp.HasAccessToLanguage(_germanLanguageId), Is.False);
        });
    }

    [Test]
    public void CheckIfHasAccessToAllLanguagesGivesAccessToAllLanguages()
    {
        IReadOnlyUserGroup userGrp = new UserGroup(new MockShortStringHelper()) { HasAccessToAllLanguages = true };

        Assert.Multiple(() =>
        {
            Assert.That(userGrp.HasAccessToLanguage(_swedishLanguageId), Is.True);
            Assert.That(userGrp.HasAccessToLanguage(_danishLanguageId), Is.True);
            Assert.That(userGrp.HasAccessToLanguage(_germanLanguageId), Is.True);
        });
    }

    private IUserGroup Build() =>
        _builder
            .WithId(3)
            .WithAllowedSections(new List<string> { "A", "B" })
            .Build();
}
