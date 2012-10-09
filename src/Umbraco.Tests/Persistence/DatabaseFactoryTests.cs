using NUnit.Framework;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class DatabaseFactoryTests
    {
        [Test]
        public void Can_Verify_Single_Database_Instance()
        {
            var db1 = DatabaseFactory.Current.Database;
            var db2 = DatabaseFactory.Current.Database;

            Assert.AreSame(db1, db2);
        }
    }
}