using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Tests.Resolvers
{
	[TestFixture]
	public class LazyManyObjectResolverTests
	{

		[SetUp]
		public void Initialize()
		{

		}

		[TearDown]
		public void TearDown()
		{
            Resolution.Unfreeze();
		}

		[Test]
		public void Ensure_Lazy_Type_Resolution()
		{
			var resolver = new LazyResolver(new[] {new Lazy<Type>(() => typeof (TransientObject3))});
			resolver.AddType<TransientObject1>();
			resolver.AddType(new Lazy<Type>(() => typeof(TransientObject2)));			

			Resolution.Freeze();

			Assert.IsFalse(resolver.HasResolvedTypes);

			var instances1 = resolver.Objects;
			
			Assert.IsTrue(resolver.HasResolvedTypes);

			Assert.AreEqual(3, instances1.Count());
			Assert.IsTrue(instances1.Select(x => x.GetType()).ContainsAll(new []{typeof(TransientObject1), typeof(TransientObject2), typeof(TransientObject3)}));
		}

		[Test]
		public void Type_List_Delegates_Combination()
		{
			Func<IEnumerable<Type>> types = () => new[] { typeof(TransientObject3), typeof(TransientObject2) };

			var resolver = new LazyResolver(types);
			resolver.AddTypeListDelegate(() => new[] { typeof(TransientObject1)});

			Resolution.Freeze();

			var instances1 = resolver.Objects;

			Assert.AreEqual(3, instances1.Count());
			Assert.IsTrue(instances1.Select(x => x.GetType()).ContainsAll(new[] { typeof(TransientObject1), typeof(TransientObject2), typeof(TransientObject3) }));
		}

		[Test]
		public void Type_List_Delegates_And_Lazy_Type_Combination()
		{
			Func<IEnumerable<Type>> types = () => new[] { typeof(TransientObject3) };

			var resolver = new LazyResolver(types);
			resolver.AddType(new Lazy<Type>(() => typeof(TransientObject2)));
			resolver.AddType<TransientObject1>();

			Resolution.Freeze();

			var instances1 = resolver.Objects;

			Assert.AreEqual(3, instances1.Count());
			Assert.IsTrue(instances1.Select(x => x.GetType()).ContainsAll(new[] { typeof(TransientObject1), typeof(TransientObject2), typeof(TransientObject3) }));
		}

		[Test]
		public void Throws_If_Duplication()
		{
			Func<IEnumerable<Type>> types = () => new[] { typeof(TransientObject3), typeof(TransientObject2), typeof(TransientObject1) };

			var resolver = new LazyResolver(types);
			//duplicate, but will not throw here
			resolver.AddType<TransientObject1>();

			Resolution.Freeze();

			Assert.Throws<InvalidOperationException>(() =>
				{
					var instances = resolver.Objects;
				});


		}

		#region Test classes

		private interface ITestInterface
		{
		}

		private class TransientObject1 : ITestInterface
		{
		}

		private class TransientObject2 : ITestInterface
		{
		}

		private class TransientObject3 : ITestInterface
		{
		}

		private sealed class LazyResolver : LazyManyObjectsResolverBase<LazyResolver, ITestInterface>
		{
			public LazyResolver()
				: base(ObjectLifetimeScope.Transient)
			{
			}

			public LazyResolver(IEnumerable<Lazy<Type>> values)
				:base (values, ObjectLifetimeScope.Transient)
			{
				
			}

			public LazyResolver(Func<IEnumerable<Type>> typeList)
				: base(typeList, ObjectLifetimeScope.Transient)
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