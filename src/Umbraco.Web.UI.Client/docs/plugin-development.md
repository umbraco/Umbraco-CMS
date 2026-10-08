
#### Adding plugin-specific localization keys

A plugin that wants its own typed localization keys needs two pieces — runtime registration and (optionally) type declaration. They're decoupled: register-only works fine, the type declaration just enables autocomplete and arg-type inference on the plugin's own keys.

**1. Register the strings at runtime** via a `localization` extension. The runtime registers them under the active culture, flattens `group.key` into `group_key`, and merges into the dictionary the controller looks up.

Inline form (best for small overrides):

```json
// umbraco-package.json
{
    "alias": "mypkg.extensions",
    "name": "MyPkg",
    "extensions": [
        {
            "type": "localization",
            "alias": "Mypkg.Localize.EnUS",
            "name": "English",
            "meta": {
                "culture": "en-US",
                "localizations": {
                    "mypkg": {
                        "anything": "Some string",
                        "greeting": "Hello, %0%!"
                    }
                }
            }
        }
    ]
}
```

For larger packs, point `js` at a separate JavaScript file (e.g. `"js": "/App_Plugins/MyPkg/en-us.js"`) that default-exports the same `{ group: { key: value } }` shape. The JS file form supports function entries that take typed arguments — string entries use the runtime `%0%` / `{0}` placeholder pattern.

**2. Declare the matching types globally** (optional, recommended) — same pattern as `UmbExtensionManifestMap`, plain interface merging, no module-path boilerplate:

```ts
// types.d.ts (or anywhere in the plugin's source tree)
declare global {
    interface UmbKnownLocalizationSet {
        mypkg_anything: string;
        mypkg_greeting: (name: string) => string;
    }
}
```

Plugin authors typically drop this in a single `types.d.ts` at the package root and ship it alongside the npm package — consumers of the plugin get autocomplete on the augmented keys automatically, with no extra config.

**3. Use them like built-in keys:**

```ts
this.localize.term('mypkg_anything');           // autocompletes alongside the built-ins
this.localize.term('mypkg_greeting', 'Alice');  // 'Alice: string' inferred from the declaration
html`<umb-localize key="mypkg_anything"></umb-localize>`;
```

Plugins that skip step 2 still work — their keys hit the `(string & {})` escape hatch in `term()`'s signature, so `localize.term('mypkg_anything')` compiles. They just lose autocomplete and arg-type inference on those keys.
