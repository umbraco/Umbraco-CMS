using System;
using System.Collections;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine.LuceneEngine;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models;

namespace UmbracoExamine.DataServices
{

    /// <summary>
    /// Data service used to query for media
    /// </summary>
	public class UmbracoMediaService : IMediaService
	{
		private readonly ServiceContext _services;

        [SecuritySafeCritical]
		public UmbracoMediaService()
			: this(ApplicationContext.Current.Services)
		{

		}

        [SecuritySafeCritical]
		public UmbracoMediaService(ServiceContext services)
		{
			_services = services;
		}

		/// <summary>
		/// This is quite an intensive operation...
		/// get all root media, then get the XML structure for all children,
		/// then run xpath against the navigator that's created
		/// </summary>
		/// <param name="xpath"></param>
		/// <returns></returns>
		[SecuritySafeCritical]
		public XDocument GetLatestMediaByXpath(string xpath)
		{
			var xmlMedia = XDocument.Parse("<media></media>");
			foreach (var m in _services.MediaService.GetRootMedia())
			{
				xmlMedia.Root.Add(m.ToXml());
			}
			var result = ((IEnumerable)xmlMedia.XPathEvaluate(xpath)).Cast<XElement>();
			return result.ToXDocument();
		}
	}
}
