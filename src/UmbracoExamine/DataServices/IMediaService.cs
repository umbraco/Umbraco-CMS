using System;
using System.Xml.Linq;
namespace UmbracoExamine.DataServices
{
    [Obsolete("This should no longer be used, latest content will be indexed by using the IMediaService directly")]
    public interface IMediaService 
    {
        [Obsolete("This should no longer be used, latest content will be indexed by using the IMediaService directly")]
        XDocument GetLatestMediaByXpath(string xpath);
    }
}
