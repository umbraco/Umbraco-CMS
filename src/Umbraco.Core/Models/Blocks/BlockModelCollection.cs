// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.ObjectModel;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Core.Models.Blocks;

/// <summary>
///     Represents a read-only collection of block items.
/// </summary>
/// <typeparam name="T">The type of block item.</typeparam>
public abstract class BlockModelCollection<T> : ReadOnlyCollection<T>
    where T : class, IBlockReference<IPublishedElement, IPublishedElement>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockModelCollection{T}" /> class.
    /// </summary>
    /// <param name="list">The list to wrap.</param>
    protected BlockModelCollection(IList<T> list) : base(list)
    {
    }

    /// <summary>
    ///     Gets the block item with the specified content key.
    /// </summary>
    /// <value>
    ///     The block item.
    /// </value>
    /// <param name="contentKey">The content key.</param>
    /// <returns>
    ///     The block item with the specified content key.
    /// </returns>
    public T? this[Guid contentKey] => this.FirstOrDefault(x => x.Content.Key == contentKey);

    /// <summary>
    ///     Gets the block item with the specified content UDI.
    /// </summary>
    /// <value>
    ///     The block item.
    /// </value>
    /// <param name="contentUdi">The content UDI.</param>
    /// <returns>
    ///     The block item with the specified content UDI.
    /// </returns>
    public T? this[Udi contentUdi] => contentUdi is GuidUdi guidUdi
        ? this.FirstOrDefault(x => x.Content.Key == guidUdi.Guid)
        : null;
}
