// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A built-in <see cref="IContentIndexHandler" /> whose fields are already covered by the index's system fields.
/// </summary>
/// <remarks>
///     A query provider building system fields itself (e.g. content type, name, dates) uses this marker to skip
///     running these handlers again, without matching on a specific handler implementation or namespace.
/// </remarks>
public interface ISystemContentIndexHandler : IContentIndexHandler
{
}
