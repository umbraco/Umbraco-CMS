using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Templates
{
	[TestFixture]
	public class MasterPageHelperTests
	{

		[TestCase(@"<%@ master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
		[TestCase(@"<%@ Master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
		[TestCase(@"<%@Master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
		[TestCase(@"<%@  Master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
		[TestCase(@"<%@master language=""C#"" masterpagefile=""~/masterpages/umbMaster.master"" autoeventwireup=""true"" %>")]
		public void IsMasterPageSyntax(string design)
		{
			Assert.IsTrue(MasterPageHelper.IsMasterPageSyntax(design));
		}

	}
}
