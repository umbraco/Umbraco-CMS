using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Resolvers
{
	[TestFixture]
	public class LazyManyObjectResolverTests
	{

		[SetUp]
		public void Initialize()
		{
            TestHelper.SetupLog4NetForTests();
 
            LazyResolver.Reset();
        }

		[TearDown]
		public void TearDown()
		{
            LazyResolver.Reset();
		}

		[Test]
		public void LazyResolverResolvesLazyTypes()
		{
			var resolver = new LazyResolver(new[] { new Lazy<Type>(() => typeof (TransientObject3)) });

			resolver.AddType<TransientObject1>();
			resolver.AddType(new Lazy<Type>(() => typeof(TransientObject2)));			

			Resolution.Freeze();

			Assert.IsFalse(resolver.HasResolvedTypes);

			var values = resolver.Objects;
			
			Assert.IsTrue(resolver.HasResolvedTypes);

            Assert.AreEqual(3, values.Count());
            Assert.IsTrue(values.Select(x => x.GetType())
                .ContainsAll(new [] { typeof(TransientObject1), typeof(TransientObject2), typeof(TransientObject3) }));
		}

		[Test]
		public void LazyResolverResolvesTypeProducers()
		{
			Func<IEnumerable<Type>> types = () => new[] { typeof(TransientObject3), typeof(TransientObject2) };

			var resolver = new LazyResolver(types);
			resolver.AddTypeListDelegate(() => new[] { typeof(TransientObject1)});

			Resolution.Freeze();

			var values = resolver.Objects;

			Assert.AreEqual(3, values.Count());
			Assert.IsTrue(values.Select(x => x.GetType())
                .ContainsAll(new[] { typeof(TransientObject1), typeof(TransientObject2), typeof(TransientObject3) }));
		}

		[Test]
		public void LazyResolverResolvesBothWays()
		{
			Func<IEnumerable<Type>> types = () => new[] { typeof(TransientObject3) };

			var resolver = new LazyResolver(types);
			resolver.AddType(new Lazy<Type>(() => typeof(TransientObject2)));
			resolver.AddType<TransientObject1>();

			Resolution.Freeze();

			var values = resolver.Objects;

			Assert.AreEqual(3, values.Count());
			Assert.IsTrue(values.Select(x => x.GetType())
                .ContainsAll(new[] { typeof(TransientObject1), typeof(TransientObject2), typeof(TransientObject3) }));
		}

		[Test]
		public void LazyResolverThrowsOnDuplicate()
		{
			Func<IEnumerable<Type>> types = () => new[] { typeof(TransientObject3), typeof(TransientObject2), typeof(TransientObject1) };

			var resolver = new LazyResolver(types);

			// duplicate, but will not throw here
			resolver.AddType<TransientObject1>();

			Resolution.Freeze();

			Assert.Throws<InvalidOperationException>(() =>
				{
					var values = resolver.Objects;
				});
		}

        [Test]
        public void LazyResolverThrowsOnInvalidType()
        {
            Func<IEnumerable<Type>> types = () => new[] { typeof(TransientObject3), typeof(TransientObject2), typeof(TransientObject1) };

            var resolver = new LazyResolver(types);

            // invalid, but will not throw here
            resolver.AddType(new Lazy<Type>(() => typeof(TransientObject4)));

            Resolution.Freeze();

            Assert.Throws<InvalidOperationException>(() =>
            {
                var values = resolver.Objects;
            });
        }

        [Test]
        public void LazyResolverSupportsRemove()
        {
            Func<IEnumerable<Type>> types = () => new[] { typeof(TransientObject3), typeof(TransientObject2), typeof(TransientObject1) };

            var resolver = new LazyResolver(types);

            resolver.RemoveType(typeof(TransientObject3));

            Resolution.Freeze();

            var values = resolver.Objects;
            Assert.IsFalse(values.Select(x => x.GetType()).Contains(typeof(TransientObject3)));
            Assert.IsTrue(values.Select(x => x.GetType()).ContainsAll(new[] { typeof(TransientObject2), typeof(TransientObject1) }));
        }
        
		#region Test classes

		private interface ITestInterface
		{ }

		private class TransientObject1 : ITestInterface
		{ }

		private class TransientObject2 : ITestInterface
		{ }

		private class TransientObject3 : ITestInterface
		{ }

        private class TransientObject4
        { }

		private sealed class LazyResolver : LazyManyObjectsResolverBase<LazyResolver, ITestInterface>
		{
			public LazyResolver()
				: base(ObjectLifetimeScope.Transient)
			{ }

			public LazyResolver(IEnumerable<Lazy<Type>> values)
				:base (values, ObjectLifetimeScope.Transient)
			{ }

            public LazyResolver(Func<IEnumerable<Type>> typeList)
                : base(typeList, ObjectLifetimeScope.Transient)
            { }

			public IEnumerable<ITestInterface> Objects
			{
				get { return Values; }
			}
		}
		
		#endregion
	}
}