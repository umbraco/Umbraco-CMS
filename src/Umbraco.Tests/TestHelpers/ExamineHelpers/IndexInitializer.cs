using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Examine;
using Examine.LuceneEngine;
using Examine.LuceneEngine.Providers;
using Lucene.Net.Analysis.Standard;
using UmbracoExamine;
using UmbracoExamine.DataServices;

namespace Umbraco.Tests.TestHelpers.ExamineHelpers
{
	/// <summary>
	/// Used internally by test classes to initialize a new index from the template
	/// </summary>
	internal class IndexInitializer
	{

		public IndexInitializer()
		{
			//ensure the umbraco.config and media.xml files exist at the location where we need to load them
			var appData = Path.Combine(TestHelper.CurrentAssemblyDirectory, "App_Data");
			Directory.CreateDirectory(appData);
			var umbConfig = Path.Combine(appData, "umbraco.config");
			File.Delete(umbConfig);
			using (var s = File.CreateText(umbConfig))
			{
				s.Write(ExamineResources.umbraco);
			}
			
			var umbMedia = Path.Combine(appData, "media.xml");
			File.Delete(umbMedia);			
			using (var s = File.CreateText(umbMedia))
			{
				s.Write(ExamineResources.media);
			}			
		}

		public UmbracoContentIndexer GetUmbracoIndexer(DirectoryInfo d)
		{
			var i = new UmbracoContentIndexer(new IndexCriteria(
														 new[]
                                                             {
                                                                 new TestIndexField { Name = "id", EnableSorting = true, Type = "Number" }, 
                                                                 new TestIndexField { Name = "version" }, 
                                                                 new TestIndexField { Name = "parentID" },
                                                                 new TestIndexField { Name = "level" },
                                                                 new TestIndexField { Name = "writerID" },
                                                                 new TestIndexField { Name = "creatorID" },
                                                                 new TestIndexField { Name = "nodeType" },
                                                                 new TestIndexField { Name = "template" },
                                                                 new TestIndexField { Name = "sortOrder", EnableSorting = true, Type = "Number"},
                                                                 new TestIndexField { Name = "createDate", EnableSorting = true, Type = "DateTime" }, 
                                                                 new TestIndexField { Name = "updateDate", EnableSorting = true, Type = "DateTime" }, 
                                                                 new TestIndexField { Name = "nodeName", EnableSorting = true },                                                                 
                                                                 new TestIndexField { Name = "urlName" }, 
                                                                 new TestIndexField { Name = "writerName" }, 
                                                                 new TestIndexField { Name = "creatorName" }, 
                                                                 new TestIndexField { Name = "nodeTypeAlias" }, 
                                                                 new TestIndexField { Name = "path" }                                                                 
                                                             },
                                                         new[]
														 	{
														 		new TestIndexField { Name = "headerText" }, 
														 		new TestIndexField { Name = "bodyText" },
														 		new TestIndexField { Name = "metaDescription" }, 
														 		new TestIndexField { Name = "metaKeywords" }, 
														 		new TestIndexField { Name = "bodyTextColOne" }, 
														 		new TestIndexField { Name = "bodyTextColTwo" }, 
														 		new TestIndexField { Name = "xmlStorageTest" }
														 	},
														    Enumerable.Empty<string>(),
                                                            Enumerable.Empty<string>(),
														 -1),
														 d,
														 new TestDataService(),
														 new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29),
														 false);

			//i.IndexSecondsInterval = 1;

			i.IndexingError += IndexingError;

			return i;
		}
		public UmbracoExamineSearcher GetUmbracoSearcher(DirectoryInfo d)
		{
			return new UmbracoExamineSearcher(d, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
		}
		
		public LuceneSearcher GetLuceneSearcher(DirectoryInfo d)
		{
			return new LuceneSearcher(d, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
		}
		
		public MultiIndexSearcher GetMultiSearcher(DirectoryInfo pdfDir, DirectoryInfo simpleDir, DirectoryInfo conventionDir, DirectoryInfo cwsDir)
		{
			var i = new MultiIndexSearcher(new[] { pdfDir, simpleDir, conventionDir, cwsDir }, new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_29));
			return i;
		}


		internal void IndexingError(object sender, IndexingErrorEventArgs e)
		{
			throw new ApplicationException(e.Message, e.InnerException);
		}

		internal class TestIndexField : IIndexField
		{
			public string Name { get; set; }
			public bool EnableSorting { get; set; }
			public string Type { get; set; }
		}

		internal class TestDataService : IDataService
		{

			public TestDataService()
			{
				ContentService = new TestContentService();
				LogService = new TestLogService();
				MediaService = new TestMediaService();
			}

			#region IDataService Members

			public IContentService ContentService { get; private set; }

			public ILogService LogService { get; private set; }

			public IMediaService MediaService { get; private set; }

			public string MapPath(string virtualPath)
			{
				return new DirectoryInfo(TestHelper.CurrentAssemblyDirectory) + "\\" + virtualPath.Replace("/", "\\");
			}

			#endregion
		}

		/// <summary>
		/// A mock data service used to return content from the XML data file created with CWS
		/// </summary>
		internal class TestContentService : IContentService
		{
			public const int ProtectedNode = 1142;

			public TestContentService()
			{
				var xmlFile = new DirectoryInfo(TestHelper.CurrentAssemblyDirectory).GetDirectories("App_Data")
					.Single()
					.GetFiles("umbraco.config")
					.Single();

				_xDoc = XDocument.Load(xmlFile.FullName);
			}

			#region IContentService Members

			/// <summary>
			/// Return the XDocument containing the xml from the umbraco.config xml file
			/// </summary>
			/// <param name="xpath"></param>
			/// <returns></returns>
			/// <remarks>
			/// This is no different in the test suite as published content
			/// </remarks>
			public XDocument GetLatestContentByXPath(string xpath)
			{
				var xdoc = XDocument.Parse("<content></content>");
				xdoc.Root.Add(_xDoc.XPathSelectElements(xpath));

				return xdoc;
			}

			/// <summary>
			/// Return the XDocument containing the xml from the umbraco.config xml file
			/// </summary>
			/// <param name="xpath"></param>
			/// <returns></returns>
			public XDocument GetPublishedContentByXPath(string xpath)
			{
				var xdoc = XDocument.Parse("<content></content>");
				xdoc.Root.Add(_xDoc.XPathSelectElements(xpath));

				return xdoc;
			}

			public string StripHtml(string value)
			{
				const string pattern = @"<(.|\n)*?>";
				return Regex.Replace(value, pattern, string.Empty);
			}

			public bool IsProtected(int nodeId, string path)
			{
				// single node is marked as protected for test indexer
				// hierarchy is not important for this test
				return nodeId == ProtectedNode;
			}

			public IEnumerable<string> GetAllUserPropertyNames()
			{
				return GetPublishedContentByXPath("//*[count(@id)>0]")
					.Root
					.Elements()
					.Select(x => x.Name.LocalName)
					.ToList();
			}

			public IEnumerable<string> GetAllSystemPropertyNames()
			{
				return new Dictionary<string, FieldIndexTypes>()
					{
						{"id", FieldIndexTypes.NOT_ANALYZED},
						{"version", FieldIndexTypes.NOT_ANALYZED},
						{"parentID", FieldIndexTypes.NOT_ANALYZED},
						{"level", FieldIndexTypes.NOT_ANALYZED},
						{"writerID", FieldIndexTypes.NOT_ANALYZED},
						{"creatorID", FieldIndexTypes.NOT_ANALYZED},
						{"nodeType", FieldIndexTypes.NOT_ANALYZED},
						{"template", FieldIndexTypes.NOT_ANALYZED},
						{"sortOrder", FieldIndexTypes.NOT_ANALYZED},
						{"createDate", FieldIndexTypes.NOT_ANALYZED},
						{"updateDate", FieldIndexTypes.NOT_ANALYZED},
						{"nodeName", FieldIndexTypes.ANALYZED},
						{"urlName", FieldIndexTypes.NOT_ANALYZED},
						{"writerName", FieldIndexTypes.ANALYZED},
						{"creatorName", FieldIndexTypes.ANALYZED},
						{"nodeTypeAlias", FieldIndexTypes.ANALYZED},
						{"path", FieldIndexTypes.NOT_ANALYZED}
					}.Select(x => x.Key);
			}

			#endregion

			private readonly XDocument _xDoc;







		}

		internal class TestLogService : ILogService
		{
			#region ILogService Members

			public string ProviderName { get; set; }

			public void AddErrorLog(int nodeId, string msg)
			{
				Trace.WriteLine("ERROR: (" + nodeId.ToString() + ") " + msg);
			}

			public void AddInfoLog(int nodeId, string msg)
			{
				Trace.WriteLine("INFO: (" + nodeId.ToString() + ") " + msg);
			}

			public void AddVerboseLog(int nodeId, string msg)
			{
				if (LogLevel == LoggingLevel.Verbose)
					Trace.WriteLine("VERBOSE: (" + nodeId.ToString() + ") " + msg);
			}

			public LoggingLevel LogLevel
			{
				get
				{
					return LoggingLevel.Verbose;
				}
				set
				{
					//do nothing
				}
			}

			#endregion
		}

		internal class TestMediaService : IMediaService
		{

			public TestMediaService()
			{
				var xmlFile = new DirectoryInfo(TestHelper.CurrentAssemblyDirectory).GetDirectories("App_Data")
					.Single()
					.GetFiles("media.xml")
					.Single();

				m_Doc = XDocument.Load(xmlFile.FullName);
			}

			#region IMediaService Members

			public System.Xml.Linq.XDocument GetLatestMediaByXpath(string xpath)
			{
				var xdoc = XDocument.Parse("<media></media>");
				xdoc.Root.Add(m_Doc.XPathSelectElements(xpath));
				return xdoc;
			}

			#endregion

			private XDocument m_Doc;
		}
	}
}
