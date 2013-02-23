using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Examine;

namespace Umbraco.Web.Search
{
    public class ExamineIndexerModel
    {
        public string Name { get; set; }
        public bool SupportsUnpublished { get; set; }
        public bool SupportsProtected { get; set; }
        public IIndexCriteria IndexCriteria { get; set; } 
    }

    /// <summary>
    /// Model to use to render the dashboard details
    /// </summary>
    public class ExamineDashboardDetails
    {
        public IEnumerable<ExamineIndexerModel> Indexers { get; set; }
    }
}
