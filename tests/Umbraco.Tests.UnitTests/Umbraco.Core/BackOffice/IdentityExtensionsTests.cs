// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.BackOffice;

public class IdentityExtensionsTests
{
    [Test]
    public void ToErrorMessage_When_Errors_Are_Null_Expect_ArgumentNullException()
    {
        IEnumerable<IdentityError> errors = null;

        Assert.Throws<ArgumentNullException>(() => errors.ToErrorMessage());
    }

    [Test]
    public void ToErrorMessage_When_Single_Error_Expect_Error_Description()
    {
        const string expectedError = "invalid something";
        var errors = new List<IdentityError> { new() { Code = "1", Description = expectedError } };

        var errorMessage = errors.ToErrorMessage();

        Assert.AreEqual(expectedError, errorMessage);
    }

    [Test]
    public void ToErrorMessage_When_Multiple_Errors_Expect_Error_Descriptions_With_Comma_Delimiter()
    {
        const string error1 = "invalid something";
        const string error2 = "invalid something else";
        var errors = new List<IdentityError>
        {
            new() { Code = "1", Description = error1 },
            new() { Code = "2", Description = error2 },
        };

        var errorMessage = errors.ToErrorMessage();

        Assert.AreEqual($"{error1}, {error2}", errorMessage);
    }
}
