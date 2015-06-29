using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Examine;
using Examine.LuceneEngine.Config;
using Umbraco.Core;

namespace UmbracoExamine
{
    internal class ExamineHelper
    {
      
        public static IndexSet GetConfiguredIndexSet(string name, System.Collections.Specialized.NameValueCollection config, string matchingVerb, Func<bool> alreadyConfiguredCheck)
        {
            //Need to check if the index set or IndexerData is specified...

            if (config["indexSet"] == null && alreadyConfiguredCheck() == false)
            {
                //if we don't have either, then we'll try to set the index set by naming conventions
                if (name.EndsWith(matchingVerb))
                {
                    var setNameByConvension = name.Remove(name.LastIndexOf(matchingVerb)) + "IndexSet";
                    //check if we can assign the index set by naming convention
                    var set = IndexSets.Instance.Sets.Cast<IndexSet>().SingleOrDefault(x => x.SetName == setNameByConvension);

                    if (set != null)
                    {
                        //we've found an index set by naming conventions :)
                        return set;
                    }
                }

                throw new ArgumentNullException("indexSet on LuceneExamineIndexer provider has not been set in configuration and/or the IndexerData property has not been explicitly set");
            }

            if (config["indexSet"] != null)
            {
                //if an index set is specified, ensure it exists and initialize the indexer based on the set

                if (IndexSets.Instance.Sets[config["indexSet"]] == null)
                {
                    throw new ArgumentException("The indexSet specified for the LuceneExamineIndexer provider does not exist");
                }
                var indexSetName = config["indexSet"];
                return IndexSets.Instance.Sets[indexSetName];
            }

            //it's already configured internally
            return null;
        }
    }
}
