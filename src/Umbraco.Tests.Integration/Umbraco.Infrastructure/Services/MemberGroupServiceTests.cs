using System;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
    public class MemberGroupServiceTests : UmbracoIntegrationTest
    {
        private IMemberGroupService MemberGroupService => GetRequiredService<IMemberGroupService>();

        /// <summary>
        /// Used to list out all ambiguous events that will require dispatching with a name
        /// </summary>
        [Test, Explicit]
        public void List_Ambiguous_Events()
        {
            var memberGroup = new MemberGroupBuilder()
                .WithName(string.Empty)
                .Build();
            Assert.Throws<InvalidOperationException>(() => MemberGroupService.Save(memberGroup));
        }
    }
}
