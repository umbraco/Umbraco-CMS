using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Web.Search
{
    public class ExamineSearcherModel
    {
        public ExamineSearcherModel()
        {
            ProviderProperties = new Dictionary<string, string>();
        }

        public string Name { get; set; }        
        public IDictionary<string, string> ProviderProperties { get; private set; }
    }

}
