using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Routing
{
    public interface IUmbracoContextUrlProviderFactory
    {
        /// <summary>
        /// Creates an instance of UrlProvider for the given Context
        /// </summary>
        /// <param name="umbracoContext">Umbraco Context</param>
        /// <returns>Url Provider</returns>
        UrlProvider Create(UmbracoContext umbracoContext);
    }
}
