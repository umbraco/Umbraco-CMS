using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Imaging.ImageSharp;

/// <summary>
/// Adds imaging support using ImageSharp/ImageSharp.Web.
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.Composing.IComposer" />
public sealed class ImageSharpComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
        => builder.AddUmbracoImageSharp();
}
