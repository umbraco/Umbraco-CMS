using System.Reflection;
using Examine;
using Examine.Search;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
///     Used to return diagnostic data for any index
/// </summary>
public class GenericIndexDiagnostics : IIndexDiagnostics
{
    private static readonly string[] _ignoreProperties = { "Description" };

    private readonly ISet<string> _idOnlyFieldSet = new HashSet<string> { "id" };
    private readonly IIndex _index;

    public GenericIndexDiagnostics(IIndex index) => _index = index;

    public int DocumentCount => -1; // unknown

    public int FieldCount => -1; // unknown

    public IReadOnlyDictionary<string, object?> Metadata
    {
        get
        {
            var result = new Dictionary<string, object?>();

            IOrderedEnumerable<PropertyInfo> props = TypeHelper
                .CachedDiscoverableProperties(_index.GetType(), mustWrite: false)
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

    public Attempt<string?> IsHealthy()
    {
        if (!_index.IndexExists())
        {
            return Attempt.Fail("Does not exist");
        }

        try
        {
            _index.Searcher.CreateQuery().ManagedQuery("test").SelectFields(_idOnlyFieldSet)
                .Execute(new QueryOptions(0, 1));
            return Attempt<string?>.Succeed(); // if we can search we'll assume it's healthy
        }
        catch (Exception e)
        {
            return Attempt.Fail($"Error: {e.Message}");
        }
    }

    public long GetDocumentCount() => -1L;

    public IEnumerable<string> GetFieldNames() => Enumerable.Empty<string>();
}
