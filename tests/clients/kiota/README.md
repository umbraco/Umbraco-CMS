# Kiota client smoke test

> ⚠️ **This client only partially works.** Kiota compiles and runs, and discriminates polymorphic content types correctly via the composed-type wrapper. However, the per-content-type property bags (e.g. `TestPagePropertiesModel`) lose their typed fields — kiota synthesizes empty `<Owner>_properties` types and only exposes `AdditionalData`. Property access from generated code is therefore untyped, in contrast to orval and hey-api.

Verifies that the Delivery API OpenAPI document produces a valid client when consumed by [Microsoft Kiota](https://learn.microsoft.com/openapi/kiota/overview), Microsoft's modern OpenAPI client generator with first-class OpenAPI 3.1 support.

## Prerequisites

- .NET 10 SDK
- The committed SQLite DB and `appsettings.json` ship with this branch and already have `Umbraco:CMS:DeliveryApi:Enabled = true`, `Umbraco:CMS:DeliveryApi:OpenApi:GenerateContentTypeSchemas = true`, and a few sample content types and items.
- Run the web app: `dotnet run --project src/Umbraco.Web.UI`. It should be reachable at `https://localhost:44339`.

## Run

```bash
dotnet run --project tests/clients/kiota
```

Each build:

1. Re-downloads the spec from the live URL into `spec/delivery.json`.
2. Restores the local kiota tool (`dotnet tool restore`).
3. Runs `dotnet kiota generate` to emit a fluent client into `Client/`.
4. Compiles and runs `Program.cs`.

## Why kiota and not nswag?

NSwag's generator currently breaks on parts of OpenAPI 3.1 (`oneOf` + `discriminator` + `const`), which our typed schemas use. Kiota was designed for 3.1 from the start and is the generator behind Microsoft Graph SDKs. This project is here to confirm a .NET client can be generated and used against the typed Delivery API spec.

## Caveats

- `Program.cs` deliberately bypasses certificate validation against `https://localhost:44339`. Don't reuse this handler in any non-throwaway code.
- The csproj passes `--disable-validation-rules all` to kiota. The Delivery API spec has two routes that share the same kiota-collapsed path signature (`/content/item/{id}` taking a Guid and `/content/item/{path}` taking a string). Without the flag, kiota refuses to generate. With it, kiota emits both as indexers on the same builder — the string overload is marked `[Obsolete]` and `Program.cs` suppresses `CS0618` to call it.
- Kiota generates polymorphic schemas as a `IComposedTypeWrapper` class with nullable properties for each variant rather than as a real interface or base type. Use `wrapper.TestPageContentResponseModel is { } testPage` (or similar) to access the populated variant.
- The properties bag (`TestPageContentResponseModel.Properties`) collapses to `AdditionalData` only — kiota does not emit strongly-typed fields for the per-property aliases. This is in contrast to orval and hey-api, which keep full type fidelity. Unclear whether this is a kiota limitation around `allOf` composition + `additionalProperties: false`, or a quirk of disabling validation. The fluent path + discriminator handling are otherwise solid.
