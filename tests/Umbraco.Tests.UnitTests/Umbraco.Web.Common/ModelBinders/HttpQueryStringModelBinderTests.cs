// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using Umbraco.Cms.Web.Common.ModelBinders;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.ModelBinders;

[TestFixture]
public class HttpQueryStringModelBinderTests
{
    [Test]
    public void Binds_Query_To_FormCollection()
    {
        // Arrange
        var bindingContext = CreateBindingContext("?foo=bar&baz=buzz");
        var binder = new HttpQueryStringModelBinder();

        // Act
        binder.BindModelAsync(bindingContext);

        // Assert
        Assert.True(bindingContext.Result.IsModelSet);

        var typedModel = bindingContext.Result.Model as FormCollection;
        Assert.IsNotNull(typedModel);
        Assert.AreEqual(typedModel["foo"], "bar");
        Assert.AreEqual(typedModel["baz"], "buzz");
    }

    [Test]
    public void Sets_Culture_Form_Value_From_Query_If_Provided()
    {
        // Arrange
        var bindingContext = CreateBindingContext("?foo=bar&baz=buzz&culture=en-gb");
        var binder = new HttpQueryStringModelBinder();

        // Act
        binder.BindModelAsync(bindingContext);

        // Assert
        Assert.True(bindingContext.Result.IsModelSet);

        var typedModel = bindingContext.Result.Model as FormCollection;
        Assert.IsNotNull(typedModel);
        Assert.AreEqual(typedModel["culture"], "en-gb");
    }

    [Test]
    public void Sets_Culture_Form_Value_From_Header_If_Not_Provided_In_Query()
    {
        // Arrange
        var bindingContext = CreateBindingContext("?foo=bar&baz=buzz");
        var binder = new HttpQueryStringModelBinder();

        // Act
        binder.BindModelAsync(bindingContext);

        // Assert
        Assert.True(bindingContext.Result.IsModelSet);

        var typedModel = bindingContext.Result.Model as FormCollection;
        Assert.IsNotNull(typedModel);
        Assert.AreEqual(typedModel["culture"], "en-gb");
    }

    private ModelBindingContext CreateBindingContext(string querystring)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString(querystring);
        httpContext.Request.Headers.Add("X-UMB-CULTURE", new StringValues("en-gb"));
        var routeData = new RouteData();
        var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
        var metadataProvider = new EmptyModelMetadataProvider();
        var routeValueDictionary = new RouteValueDictionary();
        var valueProvider = new RouteValueProvider(BindingSource.Path, routeValueDictionary);
        var modelType = typeof(FormCollection);
        return new DefaultModelBindingContext
        {
            ActionContext = actionContext,
            ModelMetadata = metadataProvider.GetMetadataForType(modelType),
            ModelName = modelType.Name,
            ValueProvider = valueProvider,
        };
    }
}
