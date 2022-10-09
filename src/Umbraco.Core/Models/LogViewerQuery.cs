using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

[Serializable]
[DataContract(IsReference = true)]
public class LogViewerQuery : EntityBase, ILogViewerQuery
{
    private string? _name;
    private string? _query;

    public LogViewerQuery(string? name, string? query)
    {
        Name = name;
        _query = query;
    }

    [DataMember]
    public string? Name
    {
        get => _name;
        set => SetPropertyValueAndDetectChanges(value, ref _name, nameof(Name));
    }

    [DataMember]
    public string? Query
    {
        get => _query;
        set => SetPropertyValueAndDetectChanges(value, ref _query, nameof(Query));
    }
}
