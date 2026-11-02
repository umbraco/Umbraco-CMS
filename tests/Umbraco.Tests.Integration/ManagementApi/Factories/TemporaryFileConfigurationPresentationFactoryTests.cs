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
/// Verifies that image file type configuration under Umbraco:CMS:Content:Imaging is surfaced by the
/// temporary file configuration presentation factory.
/// </summary>
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
