// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public partial class StringExtensionsTests
{
    [TestCase('a', true)]
    [TestCase('z', true)]
    [TestCase('m', true)]
    [TestCase('A', false)]
    [TestCase('Z', false)]
    [TestCase('M', false)]
    [TestCase('0', true)]
    [TestCase('9', true)]
    [TestCase(' ', true)]
    [TestCase('\t', true)]
    [TestCase('\n', true)]
    [TestCase('!', true)]
    [TestCase('@', true)]
    [TestCase('#', true)]
    [TestCase('$', true)]
    [TestCase('.', true)]
    [TestCase(',', true)]
    [TestCase('é', true)]
    [TestCase('É', false)]
    [TestCase('ñ', true)]
    [TestCase('Ñ', false)]
    [TestCase('ß', true)]
    [TestCase('ö', true)]
    [TestCase('Ö', false)]
    [TestCase('α', true)]
    [TestCase('Α', false)]
    [TestCase('я', true)]
    [TestCase('Я', false)]
    public void IsLowerCase_ReturnsExpectedResult(char ch, bool expected)
    {
        var result = ch.IsLowerCase();
        Assert.AreEqual(expected, result);
    }

    [TestCase('A', true)]
    [TestCase('Z', true)]
    [TestCase('M', true)]
    [TestCase('a', false)]
    [TestCase('z', false)]
    [TestCase('m', false)]
    [TestCase('0', true)]
    [TestCase('9', true)]
    [TestCase(' ', true)]
    [TestCase('\t', true)]
    [TestCase('\n', true)]
    [TestCase('!', true)]
    [TestCase('@', true)]
    [TestCase('#', true)]
    [TestCase('$', true)]
    [TestCase('.', true)]
    [TestCase(',', true)]
    [TestCase('É', true)]
    [TestCase('é', false)]
    [TestCase('Ñ', true)]
    [TestCase('ñ', false)]
    [TestCase('ß', true)]
    [TestCase('Ö', true)]
    [TestCase('ö', false)]
    [TestCase('Α', true)]
    [TestCase('α', false)]
    [TestCase('Я', true)]
    [TestCase('я', false)]
    public void IsUpperCase_ReturnsExpectedResult(char ch, bool expected)
    {
        var result = ch.IsUpperCase();
        Assert.AreEqual(expected, result);
    }
}
