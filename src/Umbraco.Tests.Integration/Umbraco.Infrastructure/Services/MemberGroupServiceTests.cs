// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
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
        [Test]
        [Explicit]
        public void List_Ambiguous_Events()
        {
            MemberGroup memberGroup = new MemberGroupBuilder()
                .WithName(string.Empty)
                .Build();
            Assert.Throws<InvalidOperationException>(() => MemberGroupService.Save(memberGroup));
        }
    }
}
