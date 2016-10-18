using System;
using System.Configuration;
using Umbraco.Web.Search.Factory;

namespace Umbraco.Web.Search
{
    public class UmbracoSearcherResolver
    {
        public static IUmbracoSearcher GetInstance(UmbracoHelper umbracoHelper)
        {
            try
            {
                var section = ConfigurationManager.GetSection("UmbracoSearcherFactorySection") as UmbracoSearcherFactorySection;

                if (section != null)
                {
                    var type = Type.GetType(section.Type);

                    if (type != null)
                    {
                        var factory = Activator.CreateInstance(type);
                        var searcherFactory = factory as IUmbracoSearcherFactory;

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

            return new ExamineSearcher(umbracoHelper);
        }
    }
}
