using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Umbraco.Web.Search
{
    [DataContract(Name = "searcher", Namespace = "")]
    public class ExamineSearcherModel
    {
        public ExamineSearcherModel()
        {
            ProviderProperties = new Dictionary<string, string>();
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "providerProperties")]
        public IDictionary<string, string> ProviderProperties { get; private set; }
    }

}
