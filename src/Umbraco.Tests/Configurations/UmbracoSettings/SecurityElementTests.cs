﻿using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests.Configurations.UmbracoSettings
{
    [TestFixture]
    public class SecurityElementTests : UmbracoSettingsTests
    {
        [Test]
        public void KeepUserLoggedIn()
        {
            Assert.IsTrue(SettingsSection.Security.KeepUserLoggedIn == true);
        }

        [Test]
        public void HideDisabledUsersInBackoffice()
        {
            Assert.IsTrue(SettingsSection.Security.HideDisabledUsersInBackoffice == false);
        }

        [Test]
        public void AllowPasswordReset()
        {
            Assert.IsTrue(SettingsSection.Security.AllowPasswordReset == true);
        }

        [Test]
        public void AuthCookieDomain()
        {
            Assert.IsTrue(SettingsSection.Security.AuthCookieDomain == null);
        }

        [Test]
        public void AuthCookieName()
        {
            Assert.IsTrue(SettingsSection.Security.AuthCookieName == "UMB_UCONTEXT");
        }
    }
}
