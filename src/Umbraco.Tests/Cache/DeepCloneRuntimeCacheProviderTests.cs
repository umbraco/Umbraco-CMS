using System;
using System.Reflection;
using System.Web;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Collections;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Tests.Collections;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class DeepCloneRuntimeCacheProviderTests : RuntimeCacheProviderTests
    {
        private DeepCloneRuntimeCacheProvider _provider;

        protected override int GetTotalItemCount
        {
            get { return HttpRuntime.Cache.Count; }
        }

        public override void Setup()
        {
            base.Setup();
            _provider = new DeepCloneRuntimeCacheProvider(new HttpRuntimeCacheProvider(HttpRuntime.Cache));
        }

        internal override ICacheProvider Provider
        {
            get { return _provider; }
        }

        internal override IRuntimeCacheProvider RuntimeProvider
        {
            get { return _provider; }
        }

        [Test]
        public void Clones_List()
        {
            var original = new DeepCloneableList<DeepCloneableListTests.TestClone>(ListCloneBehavior.Always);
            original.Add(new DeepCloneableListTests.TestClone());
            original.Add(new DeepCloneableListTests.TestClone());
            original.Add(new DeepCloneableListTests.TestClone());

            var val = _provider.GetCacheItem<DeepCloneableList<DeepCloneableListTests.TestClone>>("test", () => original);

            Assert.AreEqual(original.Count, val.Count);
            foreach (var item in val)
            {
                Assert.IsTrue(item.IsClone);
            }
        }

        [Test]
        public void Ensures_Cloned_And_Reset()
        {
            var original = new TestClass()
            {
                Name = "hello"
            };
            Assert.IsTrue(original.IsDirty());

            var val = _provider.GetCacheItem<TestClass>("test", () => original);
            
            Assert.AreNotEqual(original.CloneId, val.CloneId);
            Assert.IsFalse(val.IsDirty());
        }

        [Test]
        public void DoesNotCacheExceptions()
        {
            string value;
            Assert.Throws<Exception>(() => { value = (string)_provider.GetCacheItem("key", () => GetValue(1)); });
            Assert.Throws<Exception>(() => { value = (string)_provider.GetCacheItem("key", () => GetValue(2)); });

            // does not throw
            value = (string)_provider.GetCacheItem("key", () => GetValue(3));
            Assert.AreEqual("succ3", value);

            // cache
            value = (string)_provider.GetCacheItem("key", () => GetValue(4));
            Assert.AreEqual("succ3", value);
        }

        private static string GetValue(int i)
        {
            Console.WriteLine("get" + i);
            if (i < 3)
                throw new Exception("fail");
            return "succ" + i;
        }

        private class TestClass : TracksChangesEntityBase, IDeepCloneable
        {
            public TestClass()
            {
                CloneId = Guid.NewGuid();
            }

            private static readonly PropertyInfo WriterSelector = ExpressionHelper.GetPropertyInfo<Content, string>(x => x.Name);

            private string _name;
            public string Name
            {
                get { return _name; }
                set
                {
                    SetPropertyValueAndDetectChanges(o =>
                    {
                        _name = value;
                        return _name;
                    }, _name, WriterSelector);
                }
            }

            public Guid CloneId { get; set; }

            public object DeepClone()
            {
                var shallowClone = (TestClass)MemberwiseClone();
                DeepCloneHelper.DeepCloneRefProperties(this, shallowClone);
                shallowClone.CloneId = Guid.NewGuid();
                return shallowClone;
            }
        }
    }
}