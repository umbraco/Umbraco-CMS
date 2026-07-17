// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;

namespace Umbraco.Cms.Tests.Integration.TestServerTest;

/// <summary>
/// Regression tests ensuring that configuration supplied under Umbraco:CMS:Content:Imaging reaches the
/// temporary file configuration endpoint.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Umbraco.Cms.Api.Management.Factories.TemporaryFileConfigurationPresentationFactory"/> previously
/// read a standalone <c>ContentImagingSettings</c> options instance that was never bound to a configuration
/// section, so user configuration was silently ignored and only the compiled defaults were returned. Imaging
/// settings are now read from the bound <c>ContentSettings.Imaging</c> nested section.
/// </para>
/// </remarks>
[TestFixture]
public class TemporaryFileConfigurationImagingTests : UmbracoTestServerTestBase
{
    private const string CustomImageFileType = "ico";

    [SetUp]
    public override void Setup()
    {
        // Configure a non-default image file type under the nested imaging section before the host is built.
        InMemoryConfiguration["Umbraco:CMS:Content:Imaging:ImageFileTypes:0"] = CustomImageFileType;
        base.Setup();
    }

    [Test]
    public void TemporaryFileConfiguration_ReflectsImageFileTypesFromContentImagingSection()
    {
        using IServiceScope scope = Services.CreateScope();
        ITemporaryFileConfigurationPresentationFactory factory =
            scope.ServiceProvider.GetRequiredService<ITemporaryFileConfigurationPresentationFactory>();

        TemporaryFileConfigurationResponseModel model = factory.Create();

        Assert.That(
            model.ImageFileTypes,
            Does.Contain(CustomImageFileType),
            "ImageFileTypes should reflect configuration supplied under Umbraco:CMS:Content:Imaging.");
    }
}
