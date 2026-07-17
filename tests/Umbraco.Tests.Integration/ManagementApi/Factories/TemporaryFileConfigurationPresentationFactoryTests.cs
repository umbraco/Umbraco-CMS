// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.TemporaryFile;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Factories;

/// <summary>
/// Regression tests ensuring that configuration supplied under Umbraco:CMS:Content:Imaging reaches the
/// temporary file configuration presentation factory.
/// </summary>
/// <remarks>
/// TemporaryFileConfigurationPresentationFactory previously read a standalone ContentImagingSettings options
/// instance that was never bound to a configuration section, so user configuration was silently ignored and only
/// the compiled defaults were returned. Imaging settings are now read from the bound ContentSettings.Imaging
/// nested section.
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class TemporaryFileConfigurationPresentationFactoryTests : UmbracoIntegrationTest
{
    private const string CustomImageFileType = "ico";

    private ITemporaryFileConfigurationPresentationFactory Factory
        => GetRequiredService<ITemporaryFileConfigurationPresentationFactory>();

    protected override void SetUpTestConfiguration(IConfigurationBuilder configBuilder)
    {
        base.SetUpTestConfiguration(configBuilder);
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Umbraco:CMS:Content:Imaging:ImageFileTypes:0"] = CustomImageFileType,
        });
    }

    protected override void ConfigureTestServices(IServiceCollection services)
        => services.AddTransient<ITemporaryFileConfigurationPresentationFactory, TemporaryFileConfigurationPresentationFactory>();

    [Test]
    public void Can_Read_ImageFileTypes_From_Content_Imaging_Configuration()
    {
        TemporaryFileConfigurationResponseModel model = Factory.Create();

        Assert.That(
            model.ImageFileTypes,
            Does.Contain(CustomImageFileType),
            "ImageFileTypes should reflect configuration supplied under Umbraco:CMS:Content:Imaging.");
    }
}
