// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ShortStringHelper;

[TestFixture]
public class StringValidationTests
{
    [TestCase("someone@somewhere.com", ExpectedResult = true)]
    [TestCase("someone@somewhere.co.uk", ExpectedResult = true)]
    [TestCase("someone+tag@somewhere.net", ExpectedResult = true)]
    [TestCase("futureTLD@somewhere.fooo", ExpectedResult = true)]
    [TestCase("abc@xyz.financial", ExpectedResult = true)]
    [TestCase("admin+gmail-syntax@c.pizza", ExpectedResult = true)]
    [TestCase("admin@c.pizza", ExpectedResult = true)]
    [TestCase("fdsa", ExpectedResult = false)]
    [TestCase("fdsa@", ExpectedResult = false)]

    // IsValid can be either a powerful regex OR a dummy test,
    // and by default it depends on System.ComponentModel.DataAnnotations.AppSettings.DisableRegEx
    // which ends up using BinaryCompatibility.Current.TargetsAtLeastFramework472 so for some reason
    // in 472 we are not using the regex anymore
    //
    // it can be forced, though with an app settings
    // dataAnnotations:dataTypeAttribute:disableRegEx = false
    //
    // since Umbraco is now 4.7.2+, the setting is required for the following tests to pass

    // [TestCase("fdsa@fdsa", ExpectedResult = false)]
    // [TestCase("fdsa@fdsa.", ExpectedResult = false)]
    public bool Validate_Email_Address(string input)
    {
        var foo = new EmailAddressAttribute();

        return foo.IsValid(input);
    }
}
