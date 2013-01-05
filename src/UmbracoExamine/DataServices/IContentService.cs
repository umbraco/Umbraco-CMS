using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace UmbracoExamine.DataServices
{
    public interface IContentService 
    {
        XDocument GetLatestContentByXPath(string xpath);
        XDocument GetPublishedContentByXPath(string xpath);

        /// <summary>
        /// Returns a list of ALL properties names for all nodes defined in the data source
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllUserPropertyNames();

        /// <summary>
        /// Returns a list of ALL system property names for all nodes defined in the data source
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllSystemPropertyNames();

        string StripHtml(string value);
        bool IsProtected(int nodeId, string path);
    }
}
