// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.ObjectModel;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models.Blocks;

public abstract class BlockModelCollection<T> : ReadOnlyCollection<T>
    where T : class, IBlockReference<IPublishedElement, IPublishedElement>
{
    protected BlockModelCollection(IList<T> list) : base(list)
    {
    }


    /// <summary>
    ///     Gets the <see cref="BlockListItem" /> with the specified content key.
    /// </summary>
    /// <value>
    ///     The <see cref="BlockListItem" />.
    /// </value>
    /// <param name="contentKey">The content key.</param>
    /// <returns>
    ///     The <see cref="BlockListItem" /> with the specified content key.
    /// </returns>
    public T? this[Guid contentKey] => this.FirstOrDefault(x => x.Content.Key == contentKey);

    /// <summary>
    ///     Gets the <see cref="BlockListItem" /> with the specified content UDI.
    /// </summary>
    /// <value>
    ///     The <see cref="BlockListItem" />.
    /// </value>
    /// <param name="contentUdi">The content UDI.</param>
    /// <returns>
    ///     The <see cref="BlockListItem" /> with the specified content UDI.
    /// </returns>
    public T? this[Udi contentUdi] => contentUdi is GuidUdi guidUdi
        ? this.FirstOrDefault(x => x.Content.Key == guidUdi.Guid)
        : null;
}
