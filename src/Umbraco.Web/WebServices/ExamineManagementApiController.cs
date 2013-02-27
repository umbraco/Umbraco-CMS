using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.Providers;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Web.Mvc;
using Umbraco.Web.Search;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.WebServices
{
    public class ExamineManagementApiController : UmbracoAuthorizedApiController
    {
        /// <summary>
        /// Get the details for indexers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExamineIndexerModel> GetIndexerDetails()
        {
            var model = new List<ExamineIndexerModel>(
                ExamineManager.Instance.IndexProviderCollection.Select(indexer =>
                    {
                        var indexerModel = new ExamineIndexerModel()
                            {
                                IndexCriteria = indexer.IndexerData,
                                Name = indexer.Name
                            };
                        var props = TypeHelper.CachedDiscoverableProperties(indexer.GetType(), mustWrite: false)
                            //ignore these properties
                                              .Where(x => !new[] {"IndexerData", "Description", "WorkingFolder"}.InvariantContains(x.Name))
                                              .OrderBy(x => x.Name);
                        foreach (var p in props)
                        {
                            indexerModel.ProviderProperties.Add(p.Name, p.GetValue(indexer, null).ToString());
                        }
                        return indexerModel;
                    }));
            return model;
        }

        /// <summary>
        /// Get the details for searchers
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ExamineSearcherModel> GetSearcherDetails()
        {
            var model = new List<ExamineSearcherModel>(
                ExamineManager.Instance.SearchProviderCollection.Cast<BaseSearchProvider>().Select(searcher =>
                {
                    var indexerModel = new ExamineIndexerModel()
                    {
                        Name = searcher.Name
                    };
                    var props = TypeHelper.CachedDiscoverableProperties(searcher.GetType(), mustWrite: false)
                        //ignore these properties
                                          .Where(x => !new[] {"Description"}.InvariantContains(x.Name))
                                          .OrderBy(x => x.Name);
                    foreach (var p in props)
                    {
                        indexerModel.ProviderProperties.Add(p.Name, p.GetValue(searcher, null).ToString());
                    }
                    return indexerModel;
                }));
            return model;
        }



    }
}
