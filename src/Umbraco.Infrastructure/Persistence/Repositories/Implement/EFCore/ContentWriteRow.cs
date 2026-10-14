using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement.EFCore;

/// <summary>
///     The shared rows a publishable content write persists for one entity: the node, content and content version
///     rows, built from the entity before its own rows are written. The kind-specific entity and version rows are built by
///     the repository once the values they carry are known. The write-side counterpart of
///     <see cref="ContentRow{TEntityDto,TContentVersionDto}" />.
/// </summary>
internal sealed record ContentWriteRow(NodeDto Node, ContentDto Content, ContentVersionDto ContentVersion);
