using System;
using System.Xml.Linq;
namespace UmbracoExamine.DataServices
{
    public interface IMediaService 
    {
        XDocument GetLatestMediaByXpath(string xpath);
    }
}
