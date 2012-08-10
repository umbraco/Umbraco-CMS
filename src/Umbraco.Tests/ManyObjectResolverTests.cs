using System.Collections.Generic;
using System.Linq;
using System.Web;
using NUnit.Framework;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests
{
	[TestFixture]
	public class ManyObjectResolverTests
	{

		[SetUp]
		public void Initialize()
		{
			
		}

		[TearDown]
		public void TearDown()
		{		
			Resolution.IsFrozen = false;
		}

		[Test]
		public void Ensure_Transient_Object_Creation()
		{
			var resolver = new TransientObjectsResolver();
			resolver.AddType<TransientObject>();

			Resolution.Freeze();

			var instances1 = resolver.Objects;
			var instances2 = resolver.Objects;

			Assert.IsFalse(object.ReferenceEquals(instances1.Single(), instances2.Single()));
		}

		[Test]
		public void Ensure_Application_Object_Creation()
		{
			var resolver = new ApplicationObjectsResolver();
			resolver.AddType<TransientObject>();

			Resolution.Freeze();

			var instances1 = resolver.Objects;
			var instances2 = resolver.Objects;

			Assert.IsTrue(object.ReferenceEquals(instances1.Single(), instances2.Single()));
		}

		[Test]
		public void Ensure_HttpRequest_Object_Creation()
		{
			var httpContextFactory = new FakeHttpContextFactory("~/Home");

			var resolver = new HttpRequestObjectsResolver(httpContextFactory.HttpContext);
			resolver.AddType<TransientObject>();

			Resolution.Freeze();

			var instances1 = resolver.Objects;
			var instances2 = resolver.Objects;

			Assert.IsTrue(object.ReferenceEquals(instances1.Single(), instances2.Single()));

			//now clear the items, this is like mimicing a new request
			httpContextFactory.HttpContext.Items.Clear();

			var instances3 = resolver.Objects;
			Assert.IsFalse(object.ReferenceEquals(instances1.Single(), instances3.Single()));
		}

		#region

		private interface ITestInterface
		{
		}

		private class TransientObject : ITestInterface
		{
		}

		private sealed class TransientObjectsResolver : ManyObjectsResolverBase<TransientObjectsResolver, ITestInterface>
		{
			public TransientObjectsResolver()
				: base(ObjectLifetimeScope.Transient)
			{
				
			}
			public IEnumerable<ITestInterface> Objects
			{
				get { return Values; }
			}
		}

		private sealed class ApplicationObjectsResolver : ManyObjectsResolverBase<ApplicationObjectsResolver, ITestInterface>
		{
			public ApplicationObjectsResolver()
				: base(ObjectLifetimeScope.Application)
			{

			}
			public IEnumerable<ITestInterface> Objects
			{
				get { return Values; }
			}
		}

		private sealed class HttpRequestObjectsResolver : ManyObjectsResolverBase<HttpRequestObjectsResolver, ITestInterface>
		{
			public HttpRequestObjectsResolver(HttpContextBase httpContext)
				: base(httpContext)
			{

			}
			public IEnumerable<ITestInterface> Objects
			{
				get { return Values; }
			}
		}

		#endregion

	}
}