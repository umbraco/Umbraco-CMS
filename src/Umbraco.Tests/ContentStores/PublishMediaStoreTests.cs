using System;
using System.Collections.Generic;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Umbraco.Tests
{
	[TestFixture]
	public class PublishMediaStoreTests
	{
		[Test]
		public void Test_DictionaryDocument()
		{
			Func<int, Dictionary<string, string>> getDictionary = i => new Dictionary<string, string>()
				{
					{"id", i.ToString()},
					{"template", "testTemplate"},
					{"sortOrder", "44"},
					{"nodeName", "Testing"},
					{"urlName", "testing"},
					{"nodeTypeAlias", "myType"},
					{"nodeType", "22"},
					{"writerName", "Shannon"},
					{"creatorName", "Shannon"},
					{"writerID", "33"},
					{"creatorID", "33"},
					{"path", "1,2,3,4,5"},
					{"createDate", "2012-01-02"},
					{"level", "3"}
				};

			var dicDoc = new DefaultPublishedMediaStore.DictionaryDocument(
				getDictionary(1234),
				d => new DefaultPublishedMediaStore.DictionaryDocument(
				     	getDictionary(321),
				     	a => null,
				     	a => new List<IDocument>()),
				d => new List<IDocument>());
		}
	}
}