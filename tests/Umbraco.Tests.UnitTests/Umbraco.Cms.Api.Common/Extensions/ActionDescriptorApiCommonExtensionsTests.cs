using Microsoft.AspNetCore.Mvc.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.Extensions;

[TestFixture]
internal sealed class ActionDescriptorApiCommonExtensionsTests
{
    [Test]
    public void HasMapToApiAttribute_Uses_The_Last_MapToApi_In_Endpoint_Metadata()
    {
        var actionDescriptor = new ActionDescriptor
        {
            EndpointMetadata = [new MapToApiAttribute("management"), new MapToApiAttribute("custom")],
        };

        Assert.Multiple(() =>
        {
            Assert.That(actionDescriptor.HasMapToApiAttribute("custom"), Is.True);
            Assert.That(actionDescriptor.HasMapToApiAttribute("management"), Is.False);
        });
    }
}
