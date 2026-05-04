# NSwag client smoke test

> ⚠️ **This client does not work today.** NSwag's generator emits broken references (inline `Anonymous`, `Anonymous2`, `MultinodeTreepicker`, `MediaPicker`, `Content` types) when faced with the OpenAPI 3.1 polymorphic shape this spec uses. The project is kept here to document the failure mode; expect compile errors on first build.

Verifies that the Delivery API OpenAPI document produces a valid client when consumed by [NSwag](https://github.com/RicoSuter/NSwag) via the `NSwag.ApiDescription.Client` MSBuild integration.

## Prerequisites

- .NET 10 SDK
- The committed SQLite DB and `appsettings.json` ship with this branch and already have `Umbraco:CMS:DeliveryApi:Enabled = true`, `Umbraco:CMS:DeliveryApi:OpenApi:GenerateContentTypeSchemas = true`, and a few sample content types and items.
- Run the web app: `dotnet run --project src/Umbraco.Web.UI`. It should be reachable at `https://localhost:44339`.

## Run

```bash
dotnet run --project tests/clients/nswag
```

`dotnet build` invokes `NSwag.ApiDescription.Client` which fetches the spec from the live URL and emits `UmbracoApi.g.cs` next to the project file. The generated client is a single file with both the SDK class and the typed models.

## Caveats

- NSwag is currently maintained against OpenAPI 3.0; the document we generate is OpenAPI 3.1.1. Some 3.1-only constructs (`const`, `null` in `type` arrays, `oneOf` + `discriminator` shape) may not generate cleanly. Failures here are expected to be reported as issues against NSwag, not against the OpenAPI generation.
- The `Program.cs` deliberately bypasses certificate validation against `https://localhost:44339`. Don't reuse this handler in any non-throwaway code.

## What this proves (when it works)

- The generated C# types compile.
- The polymorphic `IApiContentResponseModel` resolves to the correct concrete model (`TestPageContentResponseModel`) via pattern matching.
- Composition properties (`SharedToggle`, `SharedString`, `SharedRadiobox`, `SharedRichText`) appear directly on the composing type's properties model.
- A real HTTP request returns data shaped according to the spec.
