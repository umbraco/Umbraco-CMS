using System.IO;
using Umbraco.Tests.TestHelpers;
using UmbracoExamine.DataServices;

namespace Umbraco.Tests.UmbracoExamine
{
	public class TestDataService : IDataService
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
}