// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using NUnit.Framework;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.BackOffice;

/// <summary>
/// Contains unit tests for <see cref="IdentityExtensions"/> in the Umbraco.Core.BackOffice namespace.
/// </summary>
public class IdentityExtensionsTests
{
    /// <summary>
    /// Tests that calling ToErrorMessage with a null errors collection throws an ArgumentNullException.
    /// </summary>
    [Test]
    public void ToErrorMessage_When_Errors_Are_Null_Expect_ArgumentNullException()
    {
        IEnumerable<IdentityError> errors = null;

        Assert.Throws<ArgumentNullException>(() => errors.ToErrorMessage());
    }

    /// <summary>
    /// Tests that when a single error is present, the error message returned matches the error description.
    /// </summary>
    [Test]
    public void ToErrorMessage_When_Single_Error_Expect_Error_Description()
    {
        const string expectedError = "invalid something";
        var errors = new List<IdentityError> { new() { Code = "1", Description = expectedError } };

        var errorMessage = errors.ToErrorMessage();

        Assert.AreEqual(expectedError, errorMessage);
    }

    /// <summary>
    /// Tests that when multiple identity errors are provided, the ToErrorMessage extension method
    /// returns a single string with error descriptions separated by a comma.
    /// </summary>
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
