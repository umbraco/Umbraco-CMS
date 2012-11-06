using System;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CSharp.RuntimeBinder;
using NUnit.Framework;
using Umbraco.Core.Dynamics;

namespace Umbraco.Tests.DynamicDocument
{
	[TestFixture]
	public class DynamicXmlTests
	{

		/// <summary>
		/// Test the current Core class
		/// </summary>
		[Test]
		public void Find_Test_Core_Class()
		{
			RunFindTest(x => new DynamicXml(x));	
		}

		/// <summary>
		/// Tests the macroEngines legacy class
		/// </summary>
		[Test]
		public void Find_Test_Legacy_Class()
		{
			RunFindTest(x => new global::umbraco.MacroEngines.DynamicXml(x));
		}

		private void RunFindTest(Func<string, dynamic> getDynamicXml)
		{
			var xmlstring = @"<test>
<item id='1' name='test 1' value='found 1'/>
<item id='2' name='test 2' value='found 2'/>
<item id='3' name='test 3' value='found 3'/>
</test>";

			dynamic dXml = getDynamicXml(xmlstring);

			var result1 = dXml.Find("@name", "test 1");
			var result2 = dXml.Find("@name", "test 2");
			var result3 = dXml.Find("@name", "test 3");
			var result4 = dXml.Find("@name", "dont find");

			Assert.AreEqual("found 1", result1.value);
			Assert.AreEqual("found 2", result2.value);
			Assert.AreEqual("found 3", result3.value);
			Assert.Throws<RuntimeBinderException>(() =>
			{
				//this will throw because result4 is not found
				var temp = result4.value;
			});
		}

	}
}