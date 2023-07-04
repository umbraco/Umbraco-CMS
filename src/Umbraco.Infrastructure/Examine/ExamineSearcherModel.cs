using System.Runtime.Serialization;

namespace Umbraco.Cms.Infrastructure.Examine;
[Obsolete("This class will be removed in v14, please check documentation of specific search provider", true)]

[DataContract(Name = "searcher", Namespace = "")]
public class ExamineSearcherModel
{
    [DataMember(Name = "name")]
    public string? Name { get; set; }
}
