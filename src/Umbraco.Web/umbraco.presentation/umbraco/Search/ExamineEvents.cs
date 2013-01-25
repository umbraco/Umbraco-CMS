using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BusinessLogic;
using Examine;
using Lucene.Net.Documents;
using umbraco.businesslogic;
using umbraco.interfaces;

namespace umbraco.presentation.umbraco.Search
{
    /// <summary>
    /// Used to wire up events for Examine
    /// </summary>
    [Obsolete("This class has been superceded by Umbraco.Web.Search.ExamineEvents. This class is no longer used and will be removed from the codebase.")]
    public class ExamineEvents : IApplicationStartupHandler
    {
    }
}