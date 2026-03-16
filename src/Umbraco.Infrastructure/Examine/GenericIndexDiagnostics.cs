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

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericIndexDiagnostics"/> class with the specified index.
    /// </summary>
    /// <param name="index">The index to run diagnostics against.</param>
    public GenericIndexDiagnostics(IIndex index) => _index = index;

    /// <summary>
    /// Gets the count of documents in the index.
    /// </summary>
    /// <remarks>unknown</remarks>
    public int DocumentCount => -1;

    /// <summary>
    /// Gets the count of fields in the generic index. Returns -1 if unknown.
    /// </summary>
    /// <remarks>unknown</remarks>
    public int FieldCount => -1;

    /// <summary>
    /// Gets a read-only dictionary containing metadata extracted from the index instance.
    /// The metadata consists of property names and their corresponding values, where the properties are those of the underlying index type, excluding any ignored properties.
    /// This metadata provides insight into the configuration and state of the index instance.
    /// </summary>
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

    /// <summary>
    /// Determines whether the index is healthy by checking for its existence and verifying that a basic search query can be executed successfully.
    /// </summary>
    /// <returns>
    /// An <see cref="Attempt{string?}"/> that succeeds if the index is healthy, or fails with an error message if not.
    /// </returns>
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

    /// <summary>Gets the count of documents in the index.</summary>
    /// <returns>The number of documents in the index, or -1 if unavailable.</returns>
    public long GetDocumentCount() => -1L;

    /// <summary>
    /// Returns an enumerable collection of field names associated with the index.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of field names. Currently, this implementation returns an empty collection.</returns>
    public IEnumerable<string> GetFieldNames() => Enumerable.Empty<string>();
}
