// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.ApplicationBuilder;

[TestFixture]
public class CspNonceExtensionsTests
{
    private const string TestNonce = "abc123XYZ";

    [Test]
    public void InjectNonceIntoDirective_InsertsNonceIntoExistingScriptSrc()
    {
        // Arrange
        var csp = "default-src 'self'; script-src 'self'; style-src 'self'";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert
        Assert.That(result, Is.EqualTo("default-src 'self'; script-src 'nonce-abc123XYZ' 'self'; style-src 'self'"));
    }

    [Test]
    public void InjectNonceIntoDirective_AppendsDirectiveWhenNotPresent()
    {
        // Arrange
        var csp = "default-src 'self'; style-src 'self'";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert
        Assert.That(result, Is.EqualTo("default-src 'self'; style-src 'self'; script-src 'nonce-abc123XYZ'"));
    }

    [Test]
    public void InjectNonceIntoDirective_HandlesDirectiveAtStart()
    {
        // Arrange
        var csp = "script-src 'self' 'unsafe-inline'; default-src 'self'";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert
        Assert.That(result, Is.EqualTo("script-src 'nonce-abc123XYZ' 'self' 'unsafe-inline'; default-src 'self'"));
    }

    [Test]
    public void InjectNonceIntoDirective_HandlesDirectiveAtEnd()
    {
        // Arrange
        var csp = "default-src 'self'; script-src 'self'";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert
        Assert.That(result, Is.EqualTo("default-src 'self'; script-src 'nonce-abc123XYZ' 'self'"));
    }

    [Test]
    public void InjectNonceIntoDirective_HandlesSingleDirective()
    {
        // Arrange
        var csp = "script-src 'self'";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert
        Assert.That(result, Is.EqualTo("script-src 'nonce-abc123XYZ' 'self'"));
    }

    [Test]
    public void InjectNonceIntoDirective_HandlesEmptyCsp()
    {
        // Arrange
        var csp = string.Empty;

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert
        Assert.That(result, Is.EqualTo("; script-src 'nonce-abc123XYZ'"));
    }

    [Test]
    public void InjectNonceIntoDirective_IsCaseInsensitive()
    {
        // Arrange
        var csp = "default-src 'self'; SCRIPT-SRC 'self'";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert
        Assert.That(result, Does.Contain("'nonce-abc123XYZ'"));
    }

    [Test]
    public void InjectNonceIntoDirective_HandlesStyleSrcDirective()
    {
        // Arrange
        var csp = "default-src 'self'; style-src 'self' 'unsafe-inline'";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "style-src", TestNonce);

        // Assert
        Assert.That(result, Is.EqualTo("default-src 'self'; style-src 'nonce-abc123XYZ' 'self' 'unsafe-inline'"));
    }

    [Test]
    public void InjectNonceIntoDirective_HandlesTrailingSemicolon()
    {
        // Arrange
        var csp = "default-src 'self'; style-src 'self';";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert
        Assert.That(result, Is.EqualTo("default-src 'self'; style-src 'self'; script-src 'nonce-abc123XYZ'"));
    }

    [Test]
    public void InjectNonceIntoDirective_HandlesMultipleUrls()
    {
        // Arrange
        var csp = "script-src 'self' https://example.com https://cdn.example.com";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert
        Assert.That(result, Is.EqualTo("script-src 'nonce-abc123XYZ' 'self' https://example.com https://cdn.example.com"));
    }

    [Test]
    public void InjectNonceIntoDirective_PreservesExistingNonces()
    {
        // Arrange - CSP already has a nonce
        var csp = "script-src 'self' 'nonce-existingNonce'";

        // Act
        var result = CspNonceExtensions.InjectNonceIntoDirective(csp, "script-src", TestNonce);

        // Assert - Both nonces should be present
        Assert.That(result, Does.Contain("'nonce-abc123XYZ'"));
        Assert.That(result, Does.Contain("'nonce-existingNonce'"));
    }
}
