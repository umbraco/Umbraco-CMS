using System;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Web;
using Examine.Config;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.web;
using Examine;
using Examine.Providers;
using umbraco.cms.businesslogic.member;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace UmbracoExamine
{
    /// <summary>
    /// An <see cref="umbraco.BusinessLogic.ApplicationBase"/> instance for wiring up Examine to the Umbraco events system
    /// </summary>
    [Obsolete("This class is no longer used and will be removed from the codebase in upcoming versions")]
    public class UmbracoEventManager : ApplicationBase
    {        
    }
}
