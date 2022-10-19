// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.WebAssets;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.AngularIntegration;

[TestFixture]
public class JsInitializationTests
{
    [Test]
    public void Parse_Main()
    {
        var result = BackOfficeJavaScriptInitializer.WriteScript("[World]", "Hello", "Blah");

        Assert.AreEqual(
            @"LazyLoad.js([World], function () {
    //we need to set the legacy UmbClientMgr path
    if ((typeof UmbClientMgr) !== ""undefined"") {
        UmbClientMgr.setUmbracoPath('Hello');
    }

    jQuery(document).ready(function () {

        angular.bootstrap(document, ['Blah']);

    });
});".StripWhitespace(),
            result.StripWhitespace());
    }
}
