# Delivery API client smoke tests

Reviewer-only branch (`v18/task/delivery-api-openapi-sample-content`). Houses a pre-seeded SQLite DB and four console projects that consume the Delivery API OpenAPI document with different generators (`orval`, `hey-api`, `kiota`, `nswag`). Not for merge.

## One-time setup

The DB ships in `src/Umbraco.Web.UI/umbraco/Data/Umbraco.Sample.sqlite.db`. To use it, point the web app at that file and enable the Delivery API.

After your first `dotnet build`, the project's auto-copy target writes `src/Umbraco.Web.UI/appsettings.json` from the template. Edit the generated file:

1. **Connection string** — change `umbracoDbDSN` to point at the committed sample DB:

   ```jsonc
   "ConnectionStrings": {
     "umbracoDbDSN": "Data Source=|DataDirectory|/Umbraco.Sample.sqlite.db;Cache=Shared;Foreign Keys=True;Pooling=True",
     "umbracoDbDSN_ProviderName": "Microsoft.Data.Sqlite"
   }
   ```

2. **Delivery API + content type schemas** — add (or merge into) the `Umbraco:CMS` section:

   ```jsonc
   "DeliveryApi": {
     "Enabled": true,
     "PublicAccess": true,
     "Media": { "Enabled": true },
     "OpenApi": { "GenerateContentTypeSchemas": true }
   }
   ```

Then run `dotnet run --project src/Umbraco.Web.UI`. The web app should boot at `https://localhost:44339`, with the seeded `testPage` available at `/`.

## Running the clients

| Project | Command | Status |
| --- | --- | --- |
| `orval/` | `npm install && npm start` | Works — full property typing |
| `hey-api/` | `npm install && npm start` | Works — full property typing |
| `kiota/` | `dotnet run` | Compiles + runs, but property bags collapse to `AdditionalData`. See project README. |
| `nswag/` | `dotnet run` | **Build fails** with ~17 errors (Anonymous, MultinodeTreepicker, MediaPicker). Documented failure of NSwag against OpenAPI 3.1 polymorphic shapes. |

Each project's own README has the per-client details and caveats.
