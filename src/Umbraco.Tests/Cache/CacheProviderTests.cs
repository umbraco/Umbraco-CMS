using System;
using System.Linq;
using System.Web.UI;
using NUnit.Framework;
using Umbraco.Core.Cache;
using umbraco;

namespace Umbraco.Tests.Cache
{
    public abstract class CacheProviderTests
    {
        internal abstract IAppCache Provider { get; }
        protected abstract int GetTotalItemCount { get; }

        [SetUp]
        public virtual void Setup()
        {

        }

        [TearDown]
        public virtual void TearDown()
        {
            Provider.Clear();
        }

        [Test]
        public void Throws_On_Reentry()
        {
            // don't run for StaticCacheProvider - not making sense
            if (GetType() == typeof (StaticCacheProviderTests))
                Assert.Ignore("Do not run for StaticCacheProvider.");

            Exception exception = null;
            var result = Provider.Get("blah", () =>
            {
                try
                {
                    var result2 = Provider.Get("blah");
                }
                catch (Exception e)
                {
                    exception = e;
                }
                return "value";
            });
            Assert.IsNotNull(exception);
            Assert.IsAssignableFrom<InvalidOperationException>(exception);
        }

        [Test]
        public void Does_Not_Cache_Exceptions()
        {
            var counter = 0;

            object result;
            try
            {
                result = Provider.Get("Blah", () =>
                    {
                        counter++;
                        throw new Exception("Do not cache this");
                    });
            }
            catch (Exception){}

            try
            {
                result = Provider.Get("Blah", () =>
                {
                    counter++;
                    throw new Exception("Do not cache this");
                });
            }
            catch (Exception){}

            Assert.Greater(counter, 1);

        }

        [Test]
        public void Ensures_Delegate_Result_Is_Cached_Once()
        {
            var counter = 0;

            object result;

            result = Provider.Get("Blah", () =>
            {
                counter++;
                return "";
            });

            result = Provider.Get("Blah", () =>
            {
                counter++;
                return "";
            });

            Assert.AreEqual(counter, 1);

        }

        [Test]
        public void Can_Get_By_Search()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new LiteralControl();
            Provider.Get("Test1", () => cacheContent1);
            Provider.Get("Tester2", () => cacheContent2);
            Provider.Get("Tes3", () => cacheContent3);
            Provider.Get("different4", () => cacheContent4);

            Assert.AreEqual(4, GetTotalItemCount);

            var result = Provider.SearchByKey("Tes");

            Assert.AreEqual(3, result.Count());
        }

        [Test]
        public void Can_Clear_By_Expression()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new LiteralControl();
            Provider.Get("TTes1t", () => cacheContent1);
            Provider.Get("Tester2", () => cacheContent2);
            Provider.Get("Tes3", () => cacheContent3);
            Provider.Get("different4", () => cacheContent4);

            Assert.AreEqual(4, GetTotalItemCount);

            Provider.ClearByRegex("^\\w+es\\d.*");

            Assert.AreEqual(2, GetTotalItemCount);
        }

        [Test]
        public void Can_Clear_By_Search()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new LiteralControl();
            Provider.Get("Test1", () => cacheContent1);
            Provider.Get("Tester2", () => cacheContent2);
            Provider.Get("Tes3", () => cacheContent3);
            Provider.Get("different4", () => cacheContent4);

            Assert.AreEqual(4, GetTotalItemCount);

            Provider.ClearByKey("Test");

            Assert.AreEqual(2, GetTotalItemCount);
        }

        [Test]
        public void Can_Clear_By_Key()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new LiteralControl();
            Provider.Get("Test1", () => cacheContent1);
            Provider.Get("Test2", () => cacheContent2);
            Provider.Get("Test3", () => cacheContent3);
            Provider.Get("Test4", () => cacheContent4);

            Assert.AreEqual(4, GetTotalItemCount);

            Provider.Clear("Test1");
            Provider.Clear("Test2");

            Assert.AreEqual(2, GetTotalItemCount);
        }

        [Test]
        public void Can_Clear_All_Items()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new LiteralControl();
            Provider.Get("Test1", () => cacheContent1);
            Provider.Get("Test2", () => cacheContent2);
            Provider.Get("Test3", () => cacheContent3);
            Provider.Get("Test4", () => cacheContent4);

            Assert.AreEqual(4, GetTotalItemCount);

            Provider.Clear();

            Assert.AreEqual(0, GetTotalItemCount);
        }

        [Test]
        public void Can_Add_When_Not_Available()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            Provider.Get("Test1", () => cacheContent1);
            Assert.AreEqual(1, GetTotalItemCount);
        }

        [Test]
        public void Can_Get_When_Available()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var result = Provider.Get("Test1", () => cacheContent1);
            var result2 = Provider.Get("Test1", () => cacheContent1);
            Assert.AreEqual(1, GetTotalItemCount);
            Assert.AreEqual(result, result2);
        }

        [Test]
        public void Can_Remove_By_Type_Name()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new LiteralControl();
            Provider.Get("Test1", () => cacheContent1);
            Provider.Get("Test2", () => cacheContent2);
            Provider.Get("Test3", () => cacheContent3);
            Provider.Get("Test4", () => cacheContent4);

            Assert.AreEqual(4, GetTotalItemCount);

            //Provider.ClearCacheObjectTypes("umbraco.MacroCacheContent");
            Provider.ClearOfType(typeof(MacroCacheContent).ToString());

            Assert.AreEqual(1, GetTotalItemCount);
        }

        [Test]
        public void Can_Remove_By_Strong_Type()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new LiteralControl();
            Provider.Get("Test1", () => cacheContent1);
            Provider.Get("Test2", () => cacheContent2);
            Provider.Get("Test3", () => cacheContent3);
            Provider.Get("Test4", () => cacheContent4);

            Assert.AreEqual(4, GetTotalItemCount);

            Provider.ClearOfType<MacroCacheContent>();

            Assert.AreEqual(1, GetTotalItemCount);
        }
    }
}
