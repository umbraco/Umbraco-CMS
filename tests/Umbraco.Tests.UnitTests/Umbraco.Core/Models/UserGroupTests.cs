// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
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
        Assert.AreNotSame(clone, item);
        Assert.AreEqual(clone, item);

        Assert.AreEqual(clone.AllowedSections.Count(), item.AllowedSections.Count());
        Assert.AreNotSame(clone.AllowedSections, item.AllowedSections);

        // Verify normal properties with reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(item, null));
        }
    }

    [Test]
    public void Can_Serialize_Without_Error()
    {
        var item = Build();

        var json = JsonConvert.SerializeObject(item);
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
            Assert.IsTrue(grp.HasAccessToLanguage(_swedishLanguageId));
            Assert.IsTrue(grp.HasAccessToLanguage(_danishLanguageId));
            Assert.IsFalse(grp.HasAccessToLanguage(_germanLanguageId));
        });
    }

    [Test]
    public void CheckIfHasAccessToAllLanguagesGivesAccessToAllLanguages()
    {
        IReadOnlyUserGroup userGrp = new UserGroup(new MockShortStringHelper()) { HasAccessToAllLanguages = true };

        Assert.Multiple(() =>
        {
            Assert.IsTrue(userGrp.HasAccessToLanguage(_swedishLanguageId));
            Assert.IsTrue(userGrp.HasAccessToLanguage(_danishLanguageId));
            Assert.IsTrue(userGrp.HasAccessToLanguage(_germanLanguageId));
        });
    }

    private IUserGroup Build() =>
        _builder
            .WithId(3)
            .WithAllowedSections(new List<string> { "A", "B" })
            .Build();
}
