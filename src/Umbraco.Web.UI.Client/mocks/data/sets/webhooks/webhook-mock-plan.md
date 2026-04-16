# Webhook Mock Data Plan

This document defines the mock webhook dataset to cover all configurable UI features.
Each webhook is designed to test a specific configuration permutation.

---

## Configurable Fields

| Field            | Type                  | Required | Notes                                                        |
|------------------|-----------------------|----------|--------------------------------------------------------------|
| `id`             | `string` (GUID)       | Yes      | Unique identifier                                            |
| `name`           | `string \| null`      | No       | Display name — can be absent                                 |
| `description`    | `string \| null`      | No       | Long-form text — can be absent                               |
| `url`            | `string`              | **Yes**  | POST endpoint                                                |
| `enabled`        | `boolean`             | Yes      | Default `true` in API model                                  |
| `events`         | `WebhookEventResponseModel[]` | **Yes (min 1)** | Event alias + name + type category         |
| `contentTypeKeys`| `Guid[]`              | No       | Empty = applies to all types; populated = filtered           |
| `headers`        | `Record<string,string>` | No    | Custom HTTP headers as key-value pairs                       |

**Event categories** (used in `eventType` field): `Content`, `Media`, `Member`, `Other`

---

## Configuration Matrix

| # | Name | Description | URL | Enabled | Events (count) | Event Type(s) | Content Type Filter | Headers (count) | Purpose |
|---|------|-------------|-----|---------|----------------|---------------|---------------------|-----------------|---------|
| 1 | _(none)_ | _(none)_ | short URL | ✅ | 1 | Content | None (all) | 0 | Minimal valid webhook |
| 2 | ✅ | ✅ | short URL | ✅ | 1 | Content | None (all) | 0 | Full name + description |
| 3 | ✅ | _(none)_ | short URL | ❌ | 1 | Content | None (all) | 0 | Disabled state |
| 4 | ✅ | ✅ | short URL | ✅ | 1 | Content | 1 type | 0 | Single content type filter |
| 5 | ✅ | _(none)_ | short URL | ✅ | 2 | Content | 2 types | 0 | Multiple content types |
| 6 | ✅ | ✅ | HTTPS URL | ✅ | 1 | Content | None (all) | 1 (auth) | Single auth header |
| 7 | ✅ | _(none)_ | HTTPS URL | ✅ | 1 | Media | None (all) | 3 | Multiple headers |
| 8 | ✅ | ✅ | short URL | ✅ | 4 | Content only | None (all) | 0 | Multiple events, one category |
| 9 | ✅ | _(none)_ | short URL | ✅ | 3 | Content + Media + Member | None (all) | 0 | Events across categories |
| 10 | ✅ | ✅ | short URL | ✅ | 3 | Media only | None (all) | 0 | Media-focused events |
| 11 | ✅ | _(none)_ | short URL | ✅ | 3 | Member only | None (all) | 0 | Member-focused events |
| 12 | ✅ | ✅ | short URL | ✅ | 3 | Other (user) | None (all) | 0 | Other/User events category |
| 13 | ✅ | ✅ | HTTPS URL | ✅ | 2 | Content | 1 type | 2 | Content type filter + headers combined |
| 14 | ✅ | ✅ | short URL | ❌ | 2 | Content | 1 type | 2 | Disabled + full configuration |
| 15 | ✅ (long) | ✅ (long) | long URL | ✅ | 1 | Content | None (all) | 0 | Edge case: long values |

---

## Webhook Definitions

Referencing document type IDs from the default mock data:
- `the-simplest-document-type-id` → "The simple document type"
- `simple-document-type-id` → "Simple Document Type"
- `all-property-editors-document-type-id` → "All property editors"

---

### 1 — Minimal Valid Webhook

Tests: bare-minimum configuration; UI should render gracefully without name or description.

| Field | Value |
|-------|-------|
| `id` | `webhook-minimal-id` |
| `name` | `null` |
| `description` | `null` |
| `url` | `https://example.com/webhook` |
| `enabled` | `true` |
| `events` | `Umbraco.ContentPublish` (Content) |
| `contentTypeKeys` | `[]` |
| `headers` | `{}` |

---

### 2 — Named with Description

Tests: name and description both populated; name shows in list view, description in detail view.

| Field | Value |
|-------|-------|
| `id` | `webhook-named-id` |
| `name` | `Content Publisher` |
| `description` | `Fires whenever content is published to the live site.` |
| `url` | `https://example.com/on-publish` |
| `enabled` | `true` |
| `events` | `Umbraco.ContentPublish` (Content) |
| `contentTypeKeys` | `[]` |
| `headers` | `{}` |

---

### 3 — Disabled Webhook

Tests: disabled toggle state; list view should visually indicate inactive status.

| Field | Value |
|-------|-------|
| `id` | `webhook-disabled-id` |
| `name` | `Disabled Integration` |
| `description` | `null` |
| `url` | `https://example.com/disabled` |
| `enabled` | `false` |
| `events` | `Umbraco.ContentSaved` (Content) |
| `contentTypeKeys` | `[]` |
| `headers` | `{}` |

---

### 4 — Single Content Type Filter

Tests: content type picker with one selection; should only list matching document types.

| Field | Value |
|-------|-------|
| `id` | `webhook-single-content-type-id` |
| `name` | `Blog Post Publisher` |
| `description` | `Only fires when a Blog Post document type is published.` |
| `url` | `https://blog.example.com/on-publish` |
| `enabled` | `true` |
| `events` | `Umbraco.ContentPublish` (Content) |
| `contentTypeKeys` | `['the-simplest-document-type-id']` |
| `headers` | `{}` |

---

### 5 — Multiple Content Types Filter

Tests: content type picker with multiple selections; shows comma-separated or chip display.

| Field | Value |
|-------|-------|
| `id` | `webhook-multi-content-type-id` |
| `name` | `Multi-type Watcher` |
| `description` | `null` |
| `url` | `https://example.com/multi-type` |
| `enabled` | `true` |
| `events` | `Umbraco.ContentPublish` (Content), `Umbraco.ContentUnpublish` (Content) |
| `contentTypeKeys` | `['the-simplest-document-type-id', 'simple-document-type-id']` |
| `headers` | `{}` |

---

### 6 — Single Auth Header

Tests: header input with one key-value pair; verifies header creation and display.

| Field | Value |
|-------|-------|
| `id` | `webhook-single-header-id` |
| `name` | `Authenticated Webhook` |
| `description` | `Sends a Bearer token with every request.` |
| `url` | `https://api.example.com/webhook` |
| `enabled` | `true` |
| `events` | `Umbraco.ContentPublish` (Content) |
| `contentTypeKeys` | `[]` |
| `headers` | `{ Authorization: 'Bearer super-secret-token' }` |

---

### 7 — Multiple Headers

Tests: header list with multiple entries; verifies that all headers are shown and editable independently.

| Field | Value |
|-------|-------|
| `id` | `webhook-multi-header-id` |
| `name` | `Multi-Header Endpoint` |
| `description` | `null` |
| `url` | `https://api.example.com/media-webhook` |
| `enabled` | `true` |
| `events` | `Umbraco.MediaSave` (Media) |
| `contentTypeKeys` | `[]` |
| `headers` | `{ 'X-API-Key': 'abc123', 'X-Environment': 'production', 'X-Client-Id': 'umbraco-cms' }` |

---

### 8 — Multiple Events, Single Category

Tests: selecting several events from the same category (Content); verifies multi-event display in list and detail view.

| Field | Value |
|-------|-------|
| `id` | `webhook-content-lifecycle-id` |
| `name` | `Content Lifecycle` |
| `description` | `Tracks the full lifecycle of a content item from save through delete.` |
| `url` | `https://example.com/content-lifecycle` |
| `enabled` | `true` |
| `events` | `Umbraco.ContentSaved` (Content), `Umbraco.ContentPublish` (Content), `Umbraco.ContentUnpublish` (Content), `Umbraco.ContentDelete` (Content) |
| `contentTypeKeys` | `[]` |
| `headers` | `{}` |

---

### 9 — Multiple Events, Cross Category

Tests: events from Content, Media, and Member categories in one webhook; verifies event grouping in modal.

| Field | Value |
|-------|-------|
| `id` | `webhook-cross-category-id` |
| `name` | `Cross-Category Notifier` |
| `description` | `null` |
| `url` | `https://example.com/all-types` |
| `enabled` | `true` |
| `events` | `Umbraco.ContentPublish` (Content), `Umbraco.MediaSave` (Media), `memberSaved` (Member) |
| `contentTypeKeys` | `[]` |
| `headers` | `{}` |

---

### 10 — Media Events

Tests: Media event type category; verifies that Media events appear and are grouped correctly.

| Field | Value |
|-------|-------|
| `id` | `webhook-media-id` |
| `name` | `Media Watcher` |
| `description` | `Monitors all media create, update, and delete operations.` |
| `url` | `https://media.example.com/webhook` |
| `enabled` | `true` |
| `events` | `Umbraco.MediaSave` (Media), `Umbraco.MediaDelete` (Media), `mediaMovedToRecycleBin` (Media) |
| `contentTypeKeys` | `[]` |
| `headers` | `{}` |

---

### 11 — Member Events

Tests: Member event type category; verifies member events are grouped correctly in the events picker.

| Field | Value |
|-------|-------|
| `id` | `webhook-member-id` |
| `name` | `Member Activity` |
| `description` | `null` |
| `url` | `https://members.example.com/webhook` |
| `enabled` | `true` |
| `events` | `memberSaved` (Member), `memberDeleted` (Member), `assignedMemberRoles` (Member) |
| `contentTypeKeys` | `[]` |
| `headers` | `{}` |

---

### 12 — Other/User Events

Tests: `Other` event type category (user security events); verifies these appear in the events picker.

| Field | Value |
|-------|-------|
| `id` | `webhook-user-events-id` |
| `name` | `User Activity Monitor` |
| `description` | `Security audit webhook that fires on login, failure, and account lock events.` |
| `url` | `https://security.example.com/webhook` |
| `enabled` | `true` |
| `events` | `userLoginSuccess` (Other), `userLoginFailed` (Other), `userLocked` (Other) |
| `contentTypeKeys` | `[]` |
| `headers` | `{}` |

---

### 13 — Content Type Filter + Auth Header (Combined)

Tests: a webhook with both content type filtering and headers configured simultaneously.

| Field | Value |
|-------|-------|
| `id` | `webhook-filtered-auth-id` |
| `name` | `Secure Filtered Webhook` |
| `description` | `Filtered to a specific content type with Bearer token authentication.` |
| `url` | `https://secure.example.com/webhook` |
| `enabled` | `true` |
| `events` | `Umbraco.ContentPublish` (Content), `Umbraco.ContentUnpublish` (Content) |
| `contentTypeKeys` | `['the-simplest-document-type-id']` |
| `headers` | `{ Authorization: 'Bearer prod-token-xyz', 'X-Environment': 'production' }` |

---

### 14 — Disabled with Full Configuration

Tests: a fully-configured webhook in the disabled state; verifies that the detail view renders all fields even when disabled.

| Field | Value |
|-------|-------|
| `id` | `webhook-disabled-full-id` |
| `name` | `Legacy Integration` |
| `description` | `Previously active integration that has been disabled. Kept for reference.` |
| `url` | `https://old-system.example.com/webhook/v1` |
| `enabled` | `false` |
| `events` | `Umbraco.ContentPublish` (Content), `Umbraco.ContentDelete` (Content) |
| `contentTypeKeys` | `['simple-document-type-id']` |
| `headers` | `{ 'X-API-Key': 'old-key-abc', Authorization: 'Bearer deprecated-token' }` |

---

### 15 — Long Values (Edge Case)

Tests: UI rendering with long name, long description, and a long URL with query parameters; verifies truncation, wrapping, and tooltip behaviour.

| Field | Value |
|-------|-------|
| `id` | `webhook-long-values-id` |
| `name` | `Very Long Webhook Name That Tests The Maximum Display Width In The List And Detail Views` |
| `description` | `This is an intentionally long description to verify that the UI handles multi-line or truncated text gracefully. It covers the scenario where an editor adds detailed documentation about what this webhook does, why it exists, and which external system it connects to.` |
| `url` | `https://example.com/webhooks/endpoint/with/a/very/deep/nested/path?env=production&version=2&client=umbraco` |
| `enabled` | `true` |
| `events` | `Umbraco.ContentPublish` (Content) |
| `contentTypeKeys` | `[]` |
| `headers` | `{}` |

---

## Feature Coverage Summary

| UI Feature | Covered by # |
|---|---|
| No name (null) | 1 |
| Name present | 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 |
| No description (null) | 1, 3, 5, 7, 9, 11 |
| Description present | 2, 4, 6, 8, 10, 12, 13, 14, 15 |
| Enabled = true | 1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 15 |
| Enabled = false | 3, 14 |
| Single event | 1, 2, 3, 4, 6, 7, 15 |
| Multiple events (same category) | 5, 8, 10, 11, 12, 13, 14 |
| Multiple events (cross category) | 9 |
| Content events | 1–9, 13, 14, 15 |
| Media events | 7, 9, 10 |
| Member events | 9, 11 |
| Other/User events | 12 |
| No content type filter | 1, 2, 3, 6, 7, 8, 9, 10, 11, 12, 15 |
| Single content type filter | 4, 13, 14 |
| Multiple content type filters | 5 |
| No headers | 1, 2, 3, 4, 5, 8, 9, 10, 11, 12, 15 |
| Single header | 6 |
| Multiple headers | 7, 13, 14 |
| Long values (edge case) | 15 |
| Disabled + full config | 14 |
| Content type + headers combined | 13 |
