// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Tests.UnitTests.AutoFixture;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Configuration.Models
{
    [TestFixture]
    public class ConnectionStringsTests
    {
        [Test]
        public void ProviderName_WhenNotExplicitlySet_HasDefaultSet()
        {
            var sut = new ConnectionStrings();
            Assert.That(sut.ProviderName, Is.EqualTo(ConnectionStrings.DefaultProviderName));
        }

        [Test]
        [AutoMoqData]
        public void ConnectionString_WhenSetterCalled_ReplacesDataDirectoryPlaceholder(string aDataDirectory)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", aDataDirectory);

            var sut = new ConnectionStrings
            {
                ConnectionString = $"{ConnectionStrings.DataDirectoryPlaceholder}/foo"
            };
            Assert.That(sut.ConnectionString, Contains.Substring($"{aDataDirectory}/foo"));
        }
    }
}
