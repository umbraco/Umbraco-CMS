// Copyright (c) Umbraco.
// See LICENSE for more details.

using AutoFixture.NUnit3;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;
using Umbraco.Cms.Web.Common.AspNetCore;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration.Models;

[TestFixture]
public class GlobalSettingsTests
{
    [InlineAutoMoqData("~/umbraco", "/", "umbraco")]
    [InlineAutoMoqData("~/umbraco", "/MyVirtualDir", "umbraco")]
    [InlineAutoMoqData("~/customPath", "/MyVirtualDir/", "umbraco")]
    [InlineAutoMoqData("~/some-wacky/nestedPath", "/MyVirtualDir", "umbraco")]
    [InlineAutoMoqData("~/some-wacky/nestedPath", "/MyVirtualDir/NestedVDir/", "umbraco")]
    public void Umbraco_Mvc_Area(
        string path,
        string rootPath,
        string outcome,
        [Frozen] IOptionsMonitor<HostingSettings> hostingSettings,
        AspNetCoreHostingEnvironment hostingEnvironment)
    {
        hostingSettings.CurrentValue.ApplicationVirtualPath = rootPath;

        var globalSettings = new GlobalSettings { UmbracoPath = path };

        Assert.AreEqual(outcome, globalSettings.GetUmbracoMvcAreaNoCache(hostingEnvironment));
    }
}
