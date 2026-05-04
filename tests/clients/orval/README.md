# Orval client smoke test

Verifies that the Delivery API OpenAPI document produces a valid client when consumed by [orval](https://orval.dev).

## Prerequisites

- Node.js 22 or later
- The committed SQLite DB and `appsettings.json` ship with this branch and already have `Umbraco:CMS:DeliveryApi:Enabled = true`, `Umbraco:CMS:DeliveryApi:OpenApi:GenerateContentTypeSchemas = true`, and a few sample content types and items.
- Run the web app: `dotnet run --project src/Umbraco.Web.UI`. It should be reachable at `https://localhost:44339`.

## Run

```bash
npm install
npm start
```

`npm start` regenerates the client from the live OpenAPI document, builds, and executes `app.ts`. The script lists the seeded content items and uses TypeScript discriminated unions on `contentType` to access type-specific properties.

## What this proves

- The generated TypeScript types compile (`tsc --build`).
- The polymorphic `IApiContentResponseModel` narrows correctly on the `contentType` discriminator.
- Composition properties (e.g. `metaDescription` from `seoMetadata`) appear on the composing type's properties model.
- A real HTTP request returns data shaped according to the spec.
