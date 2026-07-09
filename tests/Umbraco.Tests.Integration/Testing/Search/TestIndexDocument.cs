using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Tests.Integration.Testing.Search;

public record TestIndexDocument(Guid Id, UmbracoObjectTypes ObjectType, IEnumerable<Variation> Variations, IEnumerable<IndexField> Fields, ContentProtection? Protection);
