using System;
using System.ComponentModel;
using System.Data.Common;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Tests.PartialTrust;
using Umbraco.Web;
using UmbracoExamine;
using umbraco;
using umbraco.presentation;
using umbraco.presentation.nodeFactory;
using umbraco.presentation.umbraco.Search;

namespace Umbraco.Tests
{
    /// <summary>
    /// Tests for TypeHelper
    /// </summary>
    [TestFixture]
    public class TypeHelperTests : AbstractPartialTrustFixture<TypeHelperTests>
    {

        [Test]
        public void Is_Static_Class()
        {
            Assert.IsTrue(TypeHelper.IsStaticClass(typeof(TypeHelper)));
            Assert.IsFalse(TypeHelper.IsStaticClass(typeof(TypeHelperTests)));
        }

        [Test]
        public void Find_Common_Base_Class()
        {
            var t1 = TypeHelper.GetLowestBaseType(typeof (OleDbCommand),
                                                  typeof (OdbcCommand),
                                                  typeof (SqlCommand));
            Assert.IsFalse(t1.Success);

            var t2 = TypeHelper.GetLowestBaseType(typeof (OleDbCommand),
                                                  typeof (OdbcCommand),
                                                  typeof (SqlCommand),
                                                  typeof (Component));
            Assert.IsTrue(t2.Success);
            Assert.AreEqual(typeof(Component), t2.Result);

            var t3 = TypeHelper.GetLowestBaseType(typeof (OleDbCommand),
                                                  typeof (OdbcCommand),
                                                  typeof (SqlCommand),
                                                  typeof (Component),
                                                  typeof (Component).BaseType);
            Assert.IsTrue(t3.Success);
            Assert.AreEqual(typeof(MarshalByRefObject), t3.Result);

            var t4 = TypeHelper.GetLowestBaseType(typeof(OleDbCommand),
                                                   typeof(OdbcCommand),
                                                   typeof(SqlCommand),
                                                   typeof(Component),
                                                   typeof(Component).BaseType,
                                                   typeof(int));
            Assert.IsFalse(t4.Success);

            var t5 = TypeHelper.GetLowestBaseType(typeof(UmbracoEventManager));
            Assert.IsTrue(t5.Success);
            Assert.AreEqual(typeof(UmbracoEventManager), t5.Result);

            var t6 = TypeHelper.GetLowestBaseType(typeof (IApplicationEventHandler),
                                                  typeof (LegacyScheduledTasks),
                                                  typeof(CacheHelperExtensions.CacheHelperApplicationEventListener));
            Assert.IsTrue(t6.Success);
            Assert.AreEqual(typeof(IApplicationEventHandler), t6.Result);

        }

        public override void TestSetup()
        {
        }

        public override void TestTearDown()
        {
        }
    }
}