using System.Collections.Generic;
using System.Web.Mvc;
using NUnit.Framework;
using Umbraco.Core.Profiling;
using Umbraco.Web;
using Umbraco.Web.Mvc;

namespace Umbraco.Tests.BootManagers
{
	[TestFixture]
	public class WebBootManagerTests
	{
		[Test]
		public void WrapViewEngines_HasEngines_WrapsAll()
		{
			IList<IViewEngine> engines = new List<IViewEngine>
				{
					new RenderViewEngine(),
					new PluginViewEngine()
				};

			WebBootManager.WrapViewEngines(engines);

			Assert.That(engines.Count, Is.EqualTo(2));
			Assert.That(engines[0], Is.InstanceOf<ProfilingViewEngine>());
			Assert.That(engines[1], Is.InstanceOf<ProfilingViewEngine>());
		}

		[Test]
		public void WrapViewEngines_HasEngines_KeepsSortOrder()
		{
            IList<IViewEngine> engines = new List<IViewEngine>
				{
					new RenderViewEngine(),
					new PluginViewEngine()
				};

			WebBootManager.WrapViewEngines(engines);

			Assert.That(engines.Count, Is.EqualTo(2));
			Assert.That(((ProfilingViewEngine)engines[0]).Inner, Is.InstanceOf<RenderViewEngine>());
			Assert.That(((ProfilingViewEngine)engines[1]).Inner, Is.InstanceOf<PluginViewEngine>());
		}


		[Test]
		public void WrapViewEngines_HasProfiledEngine_AddsSameInstance()
		{
			var profiledEngine = new ProfilingViewEngine(new PluginViewEngine());
			IList<IViewEngine> engines = new List<IViewEngine>
				{
					profiledEngine
				};

			WebBootManager.WrapViewEngines(engines);

			Assert.That(engines[0], Is.SameAs(profiledEngine));
		}

		[Test]
		public void WrapViewEngines_CollectionIsNull_DoesNotThrow()
		{
			IList<IViewEngine> engines = null;
			Assert.DoesNotThrow(() => WebBootManager.WrapViewEngines(engines));
			Assert.That(engines, Is.Null);
		}

	}
}