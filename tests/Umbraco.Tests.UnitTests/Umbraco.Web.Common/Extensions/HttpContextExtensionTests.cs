// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Text;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Extensions
{
    [TestFixture]
    public class HttpContextExtensionTests
    {
        [Test]
        public void TryGetBasicAuthCredentials_WithoutHeader_ReturnsFalse()
        {
            var httpContext = new DefaultHttpContext();

            var result = httpContext.TryGetBasicAuthCredentials(out string _, out string _);

            Assert.IsFalse(result);
        }

        [Test]
        public void TryGetBasicAuthCredentials_WithHeader_ReturnsTrueWithCredentials()
        {
            const string Username = "fred";
            const string Password = "test";

            var httpContext = new DefaultHttpContext();
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}"));
            httpContext.Request.Headers.Add("Authorization", $"Basic {credentials}");

            bool result = httpContext.TryGetBasicAuthCredentials(out string username, out string password);

            Assert.IsTrue(result);
            Assert.AreEqual(Username, username);
            Assert.AreEqual(Password, password);
        }
    }
}
