using System.Reflection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;
using Umbraco.Search.Models;

namespace Umbraco.Search.Diagnostics;

/// <summary>
///     Used to return diagnostic data for any index
/// </summary>
public class GenericIndexDiagnostics : IIndexDiagnostics
{
    private static readonly string[] _ignoreProperties = { "Description" };

    private readonly ISet<string> _idOnlyFieldSet = new HashSet<string> { "id" };
    private readonly IUmbracoIndex? _index;
    private readonly IUmbracoSearcher? _searcher;
    public GenericIndexDiagnostics(IUmbracoIndex? getIndex, IUmbracoSearcher? getSearcher)
    {
        _index = getIndex;
        _searcher = getSearcher;
    }

    public int DocumentCount => -1; // unknown

    public int FieldCount => -1; // unknown

    public IReadOnlyDictionary<string, object?> Metadata
    {
        get
        {
            var result = new Dictionary<string, object?>();

            IOrderedEnumerable<PropertyInfo> props = TypeHelper
                .CachedDiscoverableProperties(_index?.GetType() ?? typeof(IUmbracoIndex<>), mustWrite: false)
                .Where(x => _ignoreProperties.InvariantContains(x.Name) == false)
                .OrderBy(x => x.Name);

            foreach (PropertyInfo p in props)
            {
                var val = p.GetValue(_index, null) ?? string.Empty;

                result.Add(p.Name, val);
            }

            return result;
        }
    }

    public ISearchEngine? SearchEngine { get=> _index?.SearchEngine; }

    public Attempt<HealthStatus?> IsHealthy()
    {
        if (!_index?.Exists() ?? false)
        {
            return Attempt.Fail((HealthStatus?)HealthStatus.Unhealthy,new Exception("Index doesnt exist"));
        }

        try
        {
           var result= _searcher?.Search("test", 0,1);
            return Attempt<HealthStatus?>.Succeed(HealthStatus.Healthy); // if we can search we'll assume it's healthy
        }
        catch (Exception e)
        {
            return Attempt.Fail((HealthStatus?)HealthStatus.Unhealthy,e);
        }
    }

    public long GetDocumentCount() => _index?.GetDocumentCount() ?? 0;

    public IEnumerable<string> GetFieldNames() => Enumerable.Empty<string>();
}
