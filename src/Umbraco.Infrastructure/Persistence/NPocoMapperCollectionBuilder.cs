using NPoco;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Represents a builder used to configure and manage a collection of NPoco mappers within the Umbraco CMS infrastructure.
/// </summary>
public sealed class NPocoMapperCollectionBuilder : SetCollectionBuilderBase<NPocoMapperCollectionBuilder, NPocoMapperCollection, IMapper>
{
    protected override NPocoMapperCollectionBuilder This => this;
}
