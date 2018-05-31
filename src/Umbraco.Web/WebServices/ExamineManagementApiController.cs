using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Lucene.Net.Search;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;
using Umbraco.Core.Composing;
using Umbraco.Core.Services;
using Umbraco.Examine;
using Umbraco.Web.Search;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebServices
{
    [ValidateAngularAntiForgeryToken]
    [Obsolete]
    public class ExamineManagementApiController : UmbracoAuthorizedApiController
    {
        public ExamineManagementApiController(IExamineManager examineManager, ILogger logger, IRuntimeCacheProvider runtimeCacheProvider)
        {
            _examineManager = examineManager;
            _logger = logger;
            _runtimeCacheProvider = runtimeCacheProvider;
        }

        private readonly IExamineManager _examineManager;
        private readonly ILogger _logger;
        private readonly IRuntimeCacheProvider _runtimeCacheProvider;

        /// <summary>
        /// Checks if the member internal index is consistent with the data stored in the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool CheckMembersInternalIndex()
        {
            var total = Services.MemberService.Count();

            var searcher = _examineManager.GetSearcher(Constants.Examine.InternalMemberIndexer);
            var criteria = searcher.CreateCriteria().RawQuery("__IndexType:member");
            var totalIndexed = searcher.Search(criteria);
            return total == totalIndexed.TotalItemCount;
        }

        /// <summary>
        /// Checks if the media internal index is consistent with the data stored in the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool CheckMediaInternalIndex()
        {
            var total = Services.MediaService.Count();

            var searcher = _examineManager.GetSearcher(Constants.Examine.InternalIndexer);
            var criteria = searcher.CreateCriteria().RawQuery("__IndexType:media");
            var totalIndexed = searcher.Search(criteria);
            return total == totalIndexed.TotalItemCount;
        }

        /// <summary>
        /// Checks if the content internal index is consistent with the data stored in the database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public bool CheckContentInternalIndex()
        {
            var total = Services.ContentService.Count();

            var searcher = _examineManager.GetSearcher(Constants.Examine.InternalIndexer);
            var criteria = searcher.CreateCriteria().RawQuery("__IndexType:content");
            var totalIndexed = searcher.Search(criteria);
            return total == totalIndexed.TotalItemCount;
        }
    }
}
