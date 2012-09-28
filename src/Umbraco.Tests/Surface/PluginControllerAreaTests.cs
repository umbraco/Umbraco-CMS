using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.Surface
{
	[TestFixture]
	public class PluginControllerAreaTests : BaseWebTest
	{
		protected override bool RequiresDbSetup
		{
			get { return false; }
		}

		[Test]
		public void Ensure_Same_Area1()
		{
			Assert.Throws<InvalidOperationException>(() =>
			                                         new PluginControllerArea(new PluginControllerMetadata[]
			                                         	{
															PluginController.GetMetadata(typeof(Controller1)),
															PluginController.GetMetadata(typeof(Controller2)),
															PluginController.GetMetadata(typeof(Controller3)) //not same area
			                                         	}));
		}

		[Test]
		public void Ensure_Same_Area3()
		{
			Assert.Throws<InvalidOperationException>(() =>
													 new PluginControllerArea(new PluginControllerMetadata[]
			                                         	{
															PluginController.GetMetadata(typeof(Controller1)),
															PluginController.GetMetadata(typeof(Controller2)),
															PluginController.GetMetadata(typeof(Controller4)) //no area assigned
			                                         	}));
		}

		[Test]
		public void Ensure_Same_Area2()
		{
			var area = new PluginControllerArea(new PluginControllerMetadata[]
				{
					PluginController.GetMetadata(typeof(Controller1)),
					PluginController.GetMetadata(typeof(Controller2))
				});
			Assert.Pass();
		}

		#region Test classes

		[PluginController("Area1")]
		public class Controller1 : PluginController
		{
			public Controller1(UmbracoContext umbracoContext) : base(umbracoContext)
			{
			}
		}

		[PluginController("Area1")]
		public class Controller2 : PluginController
		{
			public Controller2(UmbracoContext umbracoContext)
				: base(umbracoContext)
			{
			}
		}

		[PluginController("Area2")]
		public class Controller3 : PluginController
		{
			public Controller3(UmbracoContext umbracoContext)
				: base(umbracoContext)
			{
			}
		}

		public class Controller4 : PluginController
		{
			public Controller4(UmbracoContext umbracoContext)
				: base(umbracoContext)
			{
			}
		}

		#endregion

	}
}
