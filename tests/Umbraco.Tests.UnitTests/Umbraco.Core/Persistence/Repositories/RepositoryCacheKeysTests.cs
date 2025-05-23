// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class RepositoryCacheKeysTests
{
    [Test]
    public void GetKey_Returns_Expected_Key_For_Type()
    {
        var key = RepositoryCacheKeys.GetKey<IContent>();
        Assert.AreEqual("uRepo_IContent_", key);
    }

    [Test]
    public void GetKey_Returns_Expected_Key_For_Type_And_Id()
    {
        var key = RepositoryCacheKeys.GetKey<IContent, int>(1000);
        Assert.AreEqual("uRepo_IContent_1000", key);
    }
}
