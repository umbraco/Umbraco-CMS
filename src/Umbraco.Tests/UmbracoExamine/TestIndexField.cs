using Examine;

namespace Umbraco.Tests.UmbracoExamine
{
	public class TestIndexField : IIndexField
	{
		public string Name { get; set; }
		public bool EnableSorting { get; set; }
		public string Type { get; set; }
	}
}