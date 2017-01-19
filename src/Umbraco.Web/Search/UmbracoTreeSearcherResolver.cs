using System;
using System.Configuration;
using Umbraco.Web.Search.Factory;

namespace Umbraco.Web.Search
{
    public class UmbracoTreeSearcherResolver
    {
        public static IUmbracoTreeSearcher GetInstance(UmbracoHelper umbracoHelper)
        {
            try
            {
                var section = ConfigurationManager.GetSection("UmbracoTreeSearcherFactory") as UmbracoTreeSearcherFactory;

                if (section != null)
                {
                    var type = Type.GetType(section.Type);

                    if (type != null)
                    {
                        var factory = Activator.CreateInstance(type);
                        var searcherFactory = factory as IUmbracoTreeSearcherFactory;

                        if (searcherFactory != null)
                        {
                            return searcherFactory.CreateUmbracoSearcher();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // ToDo: Log or throw ex??
            }

            return new ExamineTreeSearcher(umbracoHelper);
        }
    }
}
