using System.Runtime.Serialization;

namespace Umbraco.Web.Search
{
    [DataContract(Name = "searcher", Namespace = "")]
    public class ExamineSearcherModel
    {
        public ExamineSearcherModel()
        {
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

    }

}
