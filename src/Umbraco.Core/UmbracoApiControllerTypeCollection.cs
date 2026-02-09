using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core;

/// <summary>
///     A collection of types representing Umbraco API controllers.
/// </summary>
[Obsolete("This will be removed in Umbraco 15.")]
public class UmbracoApiControllerTypeCollection : BuilderCollectionBase<Type>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UmbracoApiControllerTypeCollection" /> class.
    /// </summary>
    /// <param name="items">A function that provides the collection of API controller types.</param>
    public UmbracoApiControllerTypeCollection(Func<IEnumerable<Type>> items)
        : base(items)
    {
    }
}
