using System;
using System.Configuration;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class DatabaseExtensionsTest
    {
        [SetUp]
		public virtual void Initialize()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", TestHelper.CurrentAssemblyDirectory);
        }

        [Test]
        public void Can_Create_umbracoNode_Table()
        {
            var factory = DatabaseFactory.Current;

            using(Transaction transaction = factory.Database.GetTransaction())
            {
                factory.Database.CreateTable<NodeDto>();

                transaction.Complete();
            }
        }
    }
}