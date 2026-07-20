namespace Umbraco.Cms.Search.Core.Models.Configuration;

public record IndexRegistration(string IndexAlias, Type Indexer, Type Searcher);
