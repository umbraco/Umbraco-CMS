using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Examine;
using Examine.LuceneEngine.Providers;
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
        /// Get the details
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
                            .Where(x => !new[] { "IndexerData", "Description", "WorkingFolder" }.InvariantContains(x.Name))
                            .OrderBy(x => x.Name);
                        foreach (var p in props)
                        {
                            indexerModel.IndexerProperties.Add(p.Name, p.GetValue(indexer, null).ToString());
                        }
                        return indexerModel;
                    }));
            return model;
        }

        //public IEnumerable<ExamineIndexerModel> GetSearcherDetails()
        //{
        //    var model = new List<ExamineIndexerModel>(
        //        ExamineManager.Instance.IndexProviderCollection.Select(indexer =>
        //        {
        //            var indexerModel = new ExamineIndexerModel()
        //            {
        //                IndexCriteria = indexer.IndexerData,
        //                Name = indexer.Name
        //            };
        //            var props = TypeHelper.CachedDiscoverableProperties(indexer.GetType(), mustWrite: false)
        //                                  .OrderBy(x => x.Name);
        //            foreach (var p in props)
        //            {
        //                indexerModel.IndexerProperties.Add(p.Name, p.GetValue(indexer, null).ToString());
        //            }
        //            return indexerModel;
        //        }));
        //    return model;
        //}


    }
}
