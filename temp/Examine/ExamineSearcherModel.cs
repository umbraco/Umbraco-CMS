using System.Runtime.Serialization;

namespace Umbraco.Cms.Infrastructure.Examine;

[DataContract(Name = "searcher", Namespace = "")]
public class ExamineSearcherModel
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }
}
