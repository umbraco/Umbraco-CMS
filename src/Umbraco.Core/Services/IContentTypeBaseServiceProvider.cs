using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides the <see cref="IContentTypeBaseService" /> corresponding to an <see cref="IContentBase" /> object.
/// </summary>
public interface IContentTypeBaseServiceProvider
{
    /// <summary>
    ///     Gets the content type service base managing types for the specified content base.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         If <paramref name="contentBase" /> is an <see cref="IContent" />, this returns the
    ///         <see cref="IContentTypeService" />, and if it's an <see cref="IMedia" />, this returns
    ///         the <see cref="IMediaTypeService" />, etc.
    ///     </para>
    ///     <para>
    ///         Services are returned as <see cref="IContentTypeBaseService" /> and can be used
    ///         to retrieve the content / media / whatever type as <see cref="IContentTypeComposition" />.
    ///     </para>
    /// </remarks>
    IContentTypeBaseService For(IContentBase contentBase);

    /// <summary>
    ///     Gets the content type of an <see cref="IContentBase" /> object.
    /// </summary>
    IContentTypeComposition? GetContentTypeOf(IContentBase contentBase);
}
