using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a saved query for the log viewer.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class LogViewerQuery : EntityBase, ILogViewerQuery
{
    private string _name = string.Empty;
    private string _query = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LogViewerQuery" /> class.
    /// </summary>
    /// <param name="name">The name of the saved query.</param>
    /// <param name="query">The query string expression.</param>
    public LogViewerQuery(string name, string query)
    {
        Name = name;
        Query = query;
    }

    /// <inheritdoc />
    [DataMember]
    public string Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name!, nameof(Name));
    }

    /// <inheritdoc />
    [DataMember]
    public string Query
    {
        get => _query;
        set => SetPropertyValueAndDetectChanges(value, ref _query!, nameof(Query));
    }
}
