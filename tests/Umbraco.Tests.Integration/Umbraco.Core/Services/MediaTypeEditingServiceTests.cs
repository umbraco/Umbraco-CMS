using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

/// <summary>
/// Tests for the media type editing service. Please notice that a lot of functional test is covered by the content type
/// editing service tests, since these services share the same base implementation.
/// </summary>
public partial class MediaTypeEditingServiceTests : ContentTypeEditingServiceTestsBase
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        services.AddSingleton<IImageUrlGenerator, TestImageUrlGenerator>();
    }

    private class TestImageUrlGenerator : IImageUrlGenerator
    {
        public IEnumerable<string> SupportedImageFileTypes => new[] { "jpg", "gif", "png" };

        public string? GetImageUrl(ImageUrlGenerationOptions options) => options?.ImageUrl;
    }
}
