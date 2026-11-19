using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Common.OpenApi;

[TestFixture]
public class DistinctMediaTypesApiDescriptionProviderTests
{
    [Test]
    public void OnProvidersExecuted_Keeps_The_First_Format_For_Each_Response_Media_Type()
    {
        var first = new SystemTextJsonOutputFormatter(System.Text.Json.JsonSerializerOptions.Default);
        var second = new SystemTextJsonOutputFormatter(System.Text.Json.JsonSerializerOptions.Default);
        var responseType = new ApiResponseType { StatusCode = 200 };
        responseType.ApiResponseFormats.Add(new ApiResponseFormat { MediaType = "application/json", Formatter = first });
        responseType.ApiResponseFormats.Add(new ApiResponseFormat { MediaType = "text/json", Formatter = first });
        responseType.ApiResponseFormats.Add(new ApiResponseFormat { MediaType = "application/json", Formatter = second });
        responseType.ApiResponseFormats.Add(new ApiResponseFormat { MediaType = "Application/JSON", Formatter = second });
        var context = ContextWith(description => description.SupportedResponseTypes.Add(responseType));

        new DistinctMediaTypesApiDescriptionProvider().OnProvidersExecuted(context);

        CollectionAssert.AreEqual(
            new[] { "application/json", "text/json" },
            responseType.ApiResponseFormats.Select(f => f.MediaType));
        Assert.AreSame(first, responseType.ApiResponseFormats[0].Formatter);
    }

    [Test]
    public void OnProvidersExecuted_Keeps_The_First_Format_For_Each_Request_Media_Type()
    {
        var context = ContextWith(description =>
        {
            description.SupportedRequestFormats.Add(new ApiRequestFormat { MediaType = "application/json" });
            description.SupportedRequestFormats.Add(new ApiRequestFormat { MediaType = "application/json" });
            description.SupportedRequestFormats.Add(new ApiRequestFormat { MediaType = "multipart/form-data" });
        });

        new DistinctMediaTypesApiDescriptionProvider().OnProvidersExecuted(context);

        CollectionAssert.AreEqual(
            new[] { "application/json", "multipart/form-data" },
            context.Results[0].SupportedRequestFormats.Select(f => f.MediaType));
    }

    [Test]
    public void OnProvidersExecuted_Leaves_Distinct_Media_Types_Untouched()
    {
        var responseType = new ApiResponseType { StatusCode = 200 };
        responseType.ApiResponseFormats.Add(new ApiResponseFormat { MediaType = "application/json" });
        responseType.ApiResponseFormats.Add(new ApiResponseFormat { MediaType = "application/xml" });
        var context = ContextWith(description => description.SupportedResponseTypes.Add(responseType));

        new DistinctMediaTypesApiDescriptionProvider().OnProvidersExecuted(context);

        Assert.AreEqual(2, responseType.ApiResponseFormats.Count);
    }

    private static ApiDescriptionProviderContext ContextWith(Action<ApiDescription> configure)
    {
        var description = new ApiDescription { ActionDescriptor = new ActionDescriptor() };
        configure(description);
        var context = new ApiDescriptionProviderContext([]);
        context.Results.Add(description);
        return context;
    }
}
