using System.Text.Json;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using UmbracoDeliveryClient.Generated;
using UmbracoDeliveryClient.Generated.Models;

// Trust the dev cert at https://localhost:44339
var handler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
};
var httpClient = new HttpClient(handler);

var authProvider = new AnonymousAuthenticationProvider();
var requestAdapter = new HttpClientRequestAdapter(authProvider, httpClient: httpClient)
{
    BaseUrl = "https://localhost:44339",
};
var client = new UmbracoApi(requestAdapter);

// The OpenAPI spec exposes both /content/item/{id} (Guid) and /content/item/{path} (string)
// at the same kiota-collapsed path. The string indexer is marked [Obsolete]; suppressing
// the warning so we can call the by-path variant.
#pragma warning disable CS0618
IApiContentResponseModel? wrapper = await client
    .Umbraco
    .Delivery
    .Api
    .V2
    .Content
    .Item["/"]
    .GetAsync(config => config.QueryParameters.Expand = "properties[$all]");
#pragma warning restore CS0618

if (wrapper is null)
{
    Console.WriteLine("No content returned for path '/'");
    return;
}

// Kiota generates polymorphic schemas as IComposedTypeWrapper: a single class with
// nullable properties for each variant. CreateFromDiscriminatorValue populates the
// matching one based on the contentType field.
if (wrapper.TestPageContentResponseModel is { } testPage)
{
    RenderTestPage(testPage);
}
else if (wrapper.TestPageInvariantContentResponseModel is { } invariant)
{
    Console.WriteLine($"  Name: {invariant.Name}");
    Console.WriteLine($"  Path: {invariant.Route?.Path}");
}
else
{
    Console.WriteLine("Wrapper had no populated variant. Discriminator value: unknown.");
}

static void RenderTestPage(TestPageContentResponseModel content)
{
    Console.WriteLine($"  Name: {content.Name}");
    Console.WriteLine($"  Path: {content.Route?.Path}");
    Console.WriteLine($"  ContentType: {content.ContentType}");

    // Kiota does not emit strongly-typed property fields for the *PropertiesModel
    // schemas — they collapse to AdditionalData. Property access is therefore
    // untyped (in contrast to orval/hey-api which preserve every field).
    if (content.Properties?.AdditionalData is not { Count: > 0 } props)
    {
        Console.WriteLine("\n  Properties: (none)");
        return;
    }

    Console.WriteLine($"\n  Properties (untyped; {props.Count} entries):");
    foreach (KeyValuePair<string, object> kv in props.OrderBy(p => p.Key))
    {
        Console.WriteLine($"    {kv.Key}: {JsonSerializer.Serialize(kv.Value)}");
    }
}
