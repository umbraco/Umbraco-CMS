﻿using System;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
    [TestFixture]
    public class VersionExtensionTests
    {
        [TestCase(1, 0, 0, 0, "0.2147483647.2147483647.2147483647")]
        [TestCase(1, 1, 0, 0, "1.0.2147483647.2147483647")]
        [TestCase(1, 1, 1, 0, "1.1.0.2147483647")]
        [TestCase(1, 1, 1, 1, "1.1.1.0")]        
        [TestCase(0, 1, 0, 0, "0.0.2147483647.2147483647")]
        [TestCase(0, 1, 1, 0, "0.1.0.2147483647")]
        [TestCase(0, 1, 1, 1, "0.1.1.0")]
        [TestCase(0, 0, 1, 0, "0.0.0.2147483647")]
        [TestCase(0, 0, 1, 1, "0.0.1.0")]
        [TestCase(0, 0, 0, 1, "0.0.0.0")]
        [TestCase(7, 3, 0, 0, "7.2.2147483647.2147483647")]
        public void Subract_Revision(int major, int minor, int build, int rev, string outcome)
        {
            var version = new Version(major, minor, build, rev);

            var result = version.SubtractRevision();

            Assert.AreEqual(new Version(outcome), result);
        }
    }
}