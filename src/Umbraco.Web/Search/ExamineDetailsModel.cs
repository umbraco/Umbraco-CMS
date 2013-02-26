using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine;

namespace Umbraco.Web.Search
{
    public class ExamineIndexerModel
    {
        public ExamineIndexerModel()
        {
            IndexerProperties = new Dictionary<string, string>();
        }

        public string Name { get; set; }        
        public IIndexCriteria IndexCriteria { get; set; }

        public IDictionary<string, string> IndexerProperties { get; private set; }
    }

}
