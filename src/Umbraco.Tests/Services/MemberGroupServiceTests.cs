using System;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
    public class MemberGroupServiceTests : TestWithSomeContentBase
    {
        /// <summary>
        /// Used to list out all ambiguous events that will require dispatching with a name
        /// </summary>
        [Test, Explicit]
        public void List_Ambiguous_Events()
        {
            var service = ServiceContext.MemberGroupService;
            Assert.Throws<InvalidOperationException>(() => service.Save(new MemberGroup { Name = "" }));
        }
    }
}
