# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with the Examine search provider.

**See also:** [Umbraco Search CLAUDE.md](../Umbraco.Cms.Search.Core/CLAUDE.md) for the search abstractions this provider implements.

## Overview

An Examine (Lucene) implementation of the search abstractions in `Umbraco.Cms.Search.Core`. It registers the built-in document, media and member indexes, and exposes its own Management API for provider-specific data that the shared search API cannot express.

## Backoffice UI

The provider's backoffice UI lives in the backoffice client, not here, at `Umbraco.Web.UI.Client/src/packages/search-management/examine`. It adds a **"Show Fields"** entity action to search results, opening a deep-linkable sidebar modal listing a document's indexed fields.

It is gated by the `Umb.Search.Condition.IndexProviderName` condition matching this provider's name, so it appears only on indexes this provider owns. Nothing is exported from that module: it has no extension points, so it is registered through the package's bundle and is otherwise private.

Keeping it there rather than in an `/App_Plugins` bundle of its own means it is served from the cache-busted backoffice asset path (which carries a long-lived `Cache-Control`, unlike `/App_Plugins`), and needs no separate frontend build in the pipeline.

## Management API

The controllers extend `ManagementApiControllerBase` but map to their own OpenAPI document via `[MapToApi]`, and route under `examine/api/v{version}` rather than the Management API path. The document is registered in `UmbracoBuilderExtensions.AddExamineSearchProvider`, which also:

- clears `Servers`, so the committed schema does not carry the host it was fetched from
- applies `ActionNameOperationIdTransformer`, so operation IDs are the action name and the generated client gets concise method names

### Regenerating the client

`OpenApi.json` in this project is committed and is the input to the generated TypeScript client, exactly as `Umbraco.Cms.Api.Management/OpenApi.json` is for the core client. Changing a controller, its route or its models means regenerating both, in this order:

```bash
# 1. with the site running, refresh the committed schema
curl -s https://localhost:44339/umbraco/openapi/search-examine-provider.json \
  -o src/Umbraco.Cms.Search.Provider.Examine/OpenApi.json

# 2. regenerate the client from the committed schema (no running site needed)
npm --prefix src/Umbraco.Web.UI.Client run generate:server-api -w @umbraco-backoffice/search-management
```

The two steps are separate on purpose: the generator only ever reads the committed schema, so the schema and the client it produced always land in the same commit. Nothing currently fails the build if they drift, so regenerate deliberately.
