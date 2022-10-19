// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Configuration;

[TestFixture]
public class NCronTabParserTests
{
    private ICronTabParser Sut => new NCronTabParser();

    [TestCase("", ExpectedResult = false)]
    [TestCase("* * * * 1", ExpectedResult = true)]
    [TestCase("* * * * * 1", ExpectedResult = false)]
    [TestCase("* * * 1", ExpectedResult = false)]
    [TestCase("Invalid", ExpectedResult = false)]
    [TestCase("I n v a l", ExpectedResult = false)]
    [TestCase("23 0-20/2 * * *", ExpectedResult = true)]
    [TestCase("5 4 * * sun", ExpectedResult = true)]
    [TestCase("0 0,12 1 */2 *", ExpectedResult = true)]
    [TestCase("0 0 1,15 * 3", ExpectedResult = true)]
    [TestCase("5 0 * 8 *", ExpectedResult = true)]
    [TestCase("22 * * 1-5 *", ExpectedResult = true)]
    [TestCase("23 0-20/2 * * *", ExpectedResult = true)]
    [TestCase("23 0-20/2 * * sun-sat", ExpectedResult = true)]
    [TestCase("23 0-20/2 * jan-dec sun-sat", ExpectedResult = true)]
    [TestCase("* * 32 * *", ExpectedResult = false)]
    public bool IsValidCronTab(string input) => Sut.IsValidCronTab(input);
}
