using System;
using Lucene.Net.Index;
using NUnit.Framework;
using Umbraco.Core;

namespace Umbraco.Tests
{
    [TestFixture]
    public class DelegateExtensionsTests
    {

        [Test]
        public void Only_Executes_Specific_Count()
        {
            var maxTries = 5;
            var totalTries = 0;
            DelegateExtensions.RetryUntilSuccessOrMaxAttempts((currentTry) =>
            {
                totalTries = currentTry;
                return Attempt<IndexWriter>.Fail();
            }, 5, TimeSpan.FromMilliseconds(10));

            Assert.AreEqual(maxTries, totalTries);
        }

        [Test]
        public void Quits_On_Success_Count()
        {
            var maxTries = 5;
            var totalTries = 0;
            DelegateExtensions.RetryUntilSuccessOrMaxAttempts((currentTry) =>
            {
                totalTries = currentTry;
                if (totalTries == 2)
                {
                    return Attempt<string>.Succeed();
                }                
                return Attempt<string>.Fail();
            }, 5, TimeSpan.FromMilliseconds(10));

            Assert.AreEqual(2, totalTries);
        }


    }
}