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

        /// <summary>
        /// If the index is not healthy this represents the index error state
        /// </summary>
        [DataMember(Name = "error")]
        public string Error { get; set; }

        /// <summary>
        /// If the index can be open/read
        /// </summary>
        [DataMember(Name = "isHealthy")]
        public bool IsHealthy { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "providerProperties")]
        public IDictionary<string, string> ProviderProperties { get; private set; }
    }

}
