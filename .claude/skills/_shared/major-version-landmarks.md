# Major version landmarks

Structural changes between maintained Umbraco CMS major versions that change **where code lives or how it works**. Use this list when moving changes between major lines: retargeting a PR to an older branch, cherry-picking, or merging up (17 → 18 → 19 → main).

A landmark tells you where to look, not what to do. Confirm against the branches themselves (`git log origin/{older}..origin/{newer} -- <path>`, `git ls-tree origin/{branch} -- <path>`) before acting on an entry.

**Keep this current.** When a new major brings a change of this kind, add it here. An entry needs: the version and PR, the paths or types that signal it, and what the equivalent is on the older line.

## Branch facts

| Line | Dev branch | Target framework | TypeScript (client) | Migration plan starts from |
|---|---|---|---|---|
| 17 | `v17/dev` | net10.0 | 5.9 | 9.4.0 |
| 18 | `v18/dev` | net10.0 | 6.0 | 16.4.0 |
| 19 | `release/19.0`, `main` | net11.0 | 6.0 | 16.4.0 |

`version.json` on each branch is the source of truth for the version. Obsolete messages and `TODO (V{N})` comments added on a line use that line's major + 2 (root `CLAUDE.md` §6).

## Moving from a newer line to an older one (retarget, backport)

How to read the "Signals" column: if the change touches these paths or types, the landmark applies.

### Introduced in 19

| Landmark | Signals | On 17/18 |
|---|---|---|
| **Search abstraction replaces the Examine integration** (#23321, RFC 0027). Back-office search, Delivery API querying and `IPublishedContentQuery` all run on the new indexes (`Umb_PublishedContent`, `Umb_Content`, `Umb_Media`, `Umb_Members`). | `src/Umbraco.Cms.Search.Core/`, `src/Umbraco.Cms.Search.Provider.Examine/`, `src/Umbraco.Cms.Search.Extension.FileToText/`, `src/Umbraco.Core/Search/`, `Umbraco.Infrastructure/Search/` indexing services, `IndexedSearchServiceBase` | The equivalent code is in `src/Umbraco.Examine.Lucene/`, `src/Umbraco.Infrastructure/Examine/`, `Umbraco.Infrastructure/Search/` (the old indexing handlers), the `IPropertyIndexValueFactory` implementations in `PropertyEditors/`, the Management API `Indexer`/`Searcher` controllers and `ExamineManagerService`, and the Delivery API `ApiContentQueryProvider`/`QueryBuilders`. A search fix almost never retargets; the same bug on 17/18 needs its own fix in that code, or may not exist there. |
| **Cookie-only back-office authentication** (#23484), and preview authorised by that cookie (#23698). | `BackOfficeAuthPolicyBuilderExtensions`, `ConfigureBackOfficeCookieOptions`, `ISessionExpiryAccessor`, `IPreviewSessionService`; client auth context, login modal/popup, `credentials: 'include'` | 17/18 use client-side OpenIddict tokens (PKCE) with `BackOfficeTokenCookieSettings` / `ConfigureBackOfficeExposedCookieOptions`, and preview uses `IPreviewTokenGenerator` / `UserBasedPreviewTokenGenerator`. Auth and session fixes generally don't carry across; reassess them against the token model. |
| **Property editors split so the model type never depends on configuration** (#23853). New aliases: `Umbraco.Label.Text`/`.Integer`/`.BigInt`/`.Decimal`/`.DateTime`/`.Time`, `Umbraco.Slider.Range`, `Umbraco.MediaPicker.Single`, `Umbraco.UrlPicker.Single`, `Umbraco.DropDown.Single`; Multi Node Tree Picker deprecated in favour of `MultipleDocumentPicker` and `MultipleMemberPicker`. | `*LabelPropertyEditor`, `LabelPropertyEditorBase`, `Single*Configuration`, `Multiple*PickerPropertyEditor`, `SingleDropDownPropertyValueConverter`, `RangeSliderValueConverter`; `V_19_0_0` migrations | One editor per alias, with the value converter choosing the model type from configuration (`umbracoDataValueType`, `multiple`, `maxNumber`, `enableRange`). A fix in a split editor maps to the combined editor on 17/18, and its configuration-dependent branch. |
| **Min/max configuration uses a range property editor** (#23470). | `RangeConfigurationField`, `RangeConfigurationHelper`, `NumberRange`, `DecimalRange`, `RangeConfigurationValidator`; `V_19_0_0.Migrate*MinMaxToRange` | Separate min and max configuration fields. |
| **v19-scheduled obsolete members removed** (~500 `[Obsolete]` usages). | — | Usually not a blocker: 19 code calls the replacements, which also exist on 17/18. A retargeted change must not newly use members that are obsolete on the older line. Removed services include `IInstallationService`, `IUpgradeService`/upgrade check, the help page API, `PublishedContentBase` and `PublishedRequestOld`. |
| **.NET 11** | `net11.0`, newer BCL APIs | 17/18 target net10.0, so APIs (and C# language features) that are new with .NET 11 won't compile there. |

### Introduced in 18

| Landmark | Signals | On 17 |
|---|---|---|
| **Elements, with a document/element refactoring of content** (#21431). Shared publishable-content base classes now host most of the content service, repository, publishing and presentation logic for both documents and elements. | `PublishableContentServiceBase`, `ContentPublishingServiceBase`, `ContentVersionServiceBase`, `PublishableContentRepositoryBase`, `ContentVersionRepositoryBase`, `PublishableContentPresentationFactoryBase`, `IPublishableContentBase`/`PublishableContentBase`; anything `Element*` (`IElementService`, `ElementRepository`, `Controllers/Element/`, client `packages/elements/`); the Library section (client `packages/library/`) | No elements and no shared bases: the logic sits in `ContentService`, `ContentPublishingService`, `DocumentRepository`, `DocumentPresentationFactory` and similar. A fix in a shared base retargets as a fix to the document-specific class, an adaptation that needs care. An element-only fix has nothing to apply to. |
| **OpenAPI generation moved from Swashbuckle SwaggerGen to `Microsoft.AspNetCore.OpenApi`** (#21058). Swashbuckle remains for the Swagger UI only. | `ConfigureUmbracoOpenApiOptionsBase`, OpenAPI transformers (`*TransformerBase`), `Api.*/OpenApi/` | `ConfigureUmbracoSwaggerGenOptions`, schema and operation filters (`SchemaIdHandler`, `OperationIdHandler`, `SubTypesHandler`, `*DocumentFilter`). Schema-shaping changes have to be rewritten against the other API. Changes to `OpenApi.json` are always regenerated per branch, never merged by hand. |
| **Single-block lists migrated to the Single Block editor** (`V_18_0_0.MigrateSingleBlockListDataTypes`; editors interchangeable since #24144). The Single Block editor itself exists from 17 (#20098). | `Umbraco.SingleBlock`, `V_18_0_0` migrations | On 17, single-block mode may still be a Block List configuration. |
| **v18-scheduled obsolete members removed** (~500 `[Obsolete]` usages). | — | As for 19: replacements exist on 17. |
| **Client TypeScript 6** | client typecheck | 17 uses TypeScript 5.9; syntax and type behaviour that 6 accepts may not typecheck on 17. |

## Moving from an older line to a newer one (merge-up)

The same landmarks apply in reverse: a fix made on 17 against a pre-landmark structure must be re-applied to the newer structure when it merges up, not just conflict-resolved.

- **Code the newer line removed or replaced.** A 17 fix to `ContentService` may belong in `PublishableContentServiceBase` on 18+. A 17/18 fix in the Examine integration has no target on 19: check whether the bug exists in the search abstraction, and port it there if so (it may need a separate PR). A 17/18 change to SwaggerGen filters has to be rewritten as an OpenAPI transformer on 18+.
- **Obsolete members.** Members obsoleted on 17 "for removal in Umbraco 19" stay on 18 and are removed on 19. When merging into 19, drop any such member (or obsolete constructor) that the merged change adds or touches, and make sure no code on 19 still calls it.
- **Migrations.** Each line has its own `UmbracoPlan`. A migration added on an older line is placed in the newer lines' plans at the matching version (e.g. under `// To 17.6.0`). Sites that have already upgraded past that point skip it, so a newer line may also need a re-run in its current version: see `V_18_1_0.AddContentTypeIdIndexForContent`, which re-runs the 17.6 index migration for sites already on 18.0. A state that only exists on a newer line is never back-ported to an older line, because minors across lines ship together and an upgrade always goes forward.
- **Plan start points differ.** 18+ starts from 16.4.0, so migrations in `V_16_*` and earlier don't exist there.
- **Target framework.** Code merged up into 19 compiles against net11.0, where analyzers and obsoletions in the newer SDK can raise warnings the older line didn't. CI requires a warning-free build.
- **Generated files.** `OpenApi.json` and the generated `*.gen.ts` client are regenerated on the target line, never merged by hand (see the `umb-update-openapi` skill).

## Known renames and replacements

Clean-merged code from an older line can still use these older names. Obsolete names compile but do the wrong thing in tests: a Moq setup on the old member silently never matches, so the test passes without testing anything. Grep the merged additions for the left-hand names.

| Older line | Newer line | From |
|---|---|---|
| `IDatabaseCacheRepository.GetContentSource*` / `RefreshContent*` etc. | `GetDocumentSource*` / `RefreshDocument*` (alongside the element equivalents) | 18 |
| `IPublishStatusQueryService.IsDocumentPublished*` | `IDocumentPublishStatusQueryService.IsPublished*` | 18 |
| Tree controllers' sync `MapTreeItemViewModels` | `MapTreeItemViewModelsAsync` | 18 |
| `UserStartNodeTreeControllerBase` (documents, media) | Also `UserStartNodeFolderTreeControllerBase` (element tree): mirror start-node changes there | 18 |
| `IFileService` template members | `ITemplateService` | 18 |
| `IPreviewService.IsInPreview()` | `IPreviewSessionService.IsActive()` | 19 |
| Picker value converters with configuration-dependent model types | `*ValueConverterBase` and the split single/multiple converters | 19 |
| Block exposes keyed by culture and segment | Culture only (#23060) | 19 |

## Files each line keeps its own copy of

When merging, always resolve these to the destination's version, or regenerate them on the destination, even when git merges them cleanly:

- `version.json`, and the `version` field in client and acceptance-test `package.json`/`package-lock.json`: keep the destination's.
- `build/nightly-E2E-test-pipelines.yml`: the schedule's `branches.include` names the line's own dev branch.
- `src/Umbraco.Cms.Api.Management/OpenApi.json` and `*.gen.ts`: regenerate (see the `umb-update-openapi` skill).
- Localization known-keys files: regenerate when the merged changes add localization keys.
- Inline-versioned csprojs that opt out of central package management (`src/Umbraco.Web.UI/Umbraco.Web.UI.csproj`, `templates/**`) can auto-merge into duplicate `PackageReference`s, or bring back pins the newer line removed. Inspect them even when they aren't conflicted.
- `tests/**/TestFiles.Designer.cs`: a local regeneration changes the namespace and breaks the Integration build; keep the destination's copy.

## In flight (not yet on any maintained line)

- **EF Core repositories.** The migration of repositories from NPoco to EF Core is happening on feature branches (`v18/feature/ef-core-*`, `v20/feature/ef-core-element-repository-*`). Nothing has landed on 17, 18 or 19 yet. When it lands, add an entry above.
