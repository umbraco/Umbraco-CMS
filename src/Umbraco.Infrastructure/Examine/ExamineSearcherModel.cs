using System.Runtime.Serialization;

namespace Umbraco.Cms.Infrastructure.Examine;

/// <summary>
/// Represents the model used to configure and interact with an Examine searcher within the Umbraco CMS infrastructure.
/// </summary>
[DataContract(Name = "searcher", Namespace = "")]
public class ExamineSearcherModel
{
    /// <summary>
    /// Gets or sets the name of the searcher.
    /// </summary>
    [DataMember(Name = "name")]
    public string? Name { get; set; }
}
