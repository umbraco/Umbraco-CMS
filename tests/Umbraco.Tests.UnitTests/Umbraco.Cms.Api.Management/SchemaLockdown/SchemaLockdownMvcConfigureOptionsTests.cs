using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaLockdownMvcConfigureOptionsTests
{
    [Test]
    public void Configure_Adds_The_Convention_Only_Once_Per_Options_Instance()
    {
        var accessor = new SchemaLockdownMatrixAccessor(
            Options.Create(new SchemaLockdownSettings()),
            new SchemaLockdownConfiguratorCollection(() => []));
        var convention = new SchemaLockdownConvention(accessor);
        var configureOptions = new SchemaLockdownMvcConfigureOptions(convention);
        var options = new MvcOptions();

        var countBefore = options.Conventions.Count;

        configureOptions.Configure(options);
        var countAfterFirstCall = options.Conventions.Count;

        configureOptions.Configure(options);
        var countAfterSecondCall = options.Conventions.Count;

        Assert.Multiple(() =>
        {
            Assert.That(countAfterFirstCall, Is.EqualTo(countBefore + 1));
            Assert.That(countAfterSecondCall, Is.EqualTo(countAfterFirstCall));
        });
    }
}
