using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.ContentTypeEditing;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Testing;

public abstract class UmbracoIntegrationTestWithMediaEditing : UmbracoIntegrationTest
{
    protected IMediaTypeEditingService MediaTypeEditingService => GetRequiredService<IMediaTypeEditingService>();
    protected IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

    protected IMediaEditingService MediaEditingService => GetRequiredService<IMediaEditingService>();

    // protected IMediaTypeContainerService MediaTypeContainerService => GetRequiredService<IMediaTypeContainerService>();



    protected MediaCreateModel RootFolder { get; private set; }

    protected MediaCreateModel RootImage { get; private set; }

    protected MediaCreateModel SubFolder1 { get; private set; }

    protected MediaCreateModel SubFolder2 { get; private set; }

    protected MediaCreateModel SubSubImage { get; private set; }

    protected MediaCreateModel SubSubFile { get; private set; }



    [SetUp]
    public new void Setup() => CreateTestData();

    protected async void CreateTestData()
    {
        var tester = MediaTypeEditingBuilder.CreateBasicMediaType();

        await MediaTypeEditingService.CreateAsync(tester, Constants.Security.SuperUserKey);
        // RootFolder =  MediaEditingBuilder.CreateBasicContent()
    }
}
