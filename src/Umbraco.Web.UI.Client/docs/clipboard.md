# Clipboard

**Module**: `@umbraco-cms/backoffice/clipboard`
**Location**: `src/packages/clipboard/`

Depends on: [Manifests & Aliases](./manifests.md)

---

Lets a user copy the value of one property editor and paste it into another — including a *different* property editor, as long as both speak a common intermediate format. This doc describes the concepts. To implement clipboard support for a property editor, use the **`general-add-clipboard-support`** skill.

## The model

A clipboard **entry** is a stored item with a name, an icon, and one or more **values**. Each value is tagged with a **clipboard entry value type** — a named, editor-agnostic format.

```
property value ──(copy translator)──▶ entry value ──(paste translator)──▶ property value
    editor A                        shared format                          editor B
```

The entry value type is the interop contract, and it is the only thing the two editors have in common. Neither knows the other exists; each only knows how to convert between its own value and the format.

## Translators are keyed on the property editor UI alias

Two extension types, and the direction is in the manifest:

| Extension type | Keyed on | Produces |
|---|---|---|
| `clipboardCopyPropertyValueTranslator` | `fromPropertyEditorUi` + `toClipboardEntryValueType` | an entry value from a property value |
| `clipboardPastePropertyValueTranslator` | `fromClipboardEntryValueType` + `toPropertyEditorUi` | a property value from an entry value |

Two consequences follow from this keying, and most mistakes come from missing them:

**Anything touching a clipboard value must sit where that property editor's value is owned.** A paste translator resolves to *a specific property editor's* value; a copy translator consumes one. So an input component — which typically has its own, different value shape and is often shared between several property editors — must never produce or consume a clipboard value. See "Where the work belongs".

**A copy runs every matching copy translator.** Register two for the same property editor and one copy produces one entry carrying both values, and an entry with several values can be pasted by any editor that understands *any one* of them.

This is the mechanism for interop, and it decides how widely a copy can travel. An editor whose value is richer than a common format should register a translator for both: the rich format so it round-trips losslessly into its own kind, and the common one so simpler editors can accept it. Registering only the rich format makes the entry unpasteable anywhere else.

## Availability is two facts

A property editor can be copyable without being pasteable, or the reverse. Both require an alias **and** at least one translator in that direction:

- **copy possible** ⇔ the alias resolves and ≥ 1 copy translator has `fromPropertyEditorUi === alias`
- **paste possible** ⇔ the alias resolves and ≥ 1 paste translator has `toPropertyEditorUi === alias`

Gate every clipboard affordance on the matching one. `UmbClipboardPropertyContext` exposes both as observables.

## `UmbClipboardPropertyContext`

Registered as a `propertyContext` extension of `kind: 'clipboard'` with `forPropertyEditorUis`, so it exists **only inside `umb-property`** for the editors that opt in. Consumed via `UMB_CLIPBOARD_PROPERTY_CONTEXT`, and absent for editors that don't register it — which is how you detect "no clipboard here".

It owns all translator wiring: selecting translators, resolving values, naming entries, and reporting availability. It **derives the property editor UI alias from the surrounding property context itself**, so callers inside a property normally pass no alias. The alias parameter remains for the case where a property editor performs a clipboard operation on behalf of nested items it hosts, and must name the alias whose translators apply rather than its own surroundings.

It also derives the entry name (`workspace - property - item`) and defaults the icon to the property editor's, so a caller usually supplies only a value.

## Where the work belongs

> **Whoever owns the value shape decides *what* is copied or pasted. Whoever owns the surface decides *where*.**

For the shapes in use today those coincide, and the property editor does both:

| Value shape | Paste means |
|---|---|
| A collection of identifiable items | merge into the collection, deduplicated |
| A single value | replace |

They only separate for a value that is edited through a **live surface** — one where an editor instance, not the value, holds the current state. There, a paste means "insert at a position", which a value snapshot cannot express, and writing a new value back would replace the document and discard the selection and undo history. No property editor does this yet; treat it as unsolved rather than assuming this doc covers it.

An input component reports and never resolves. Two events carry that:

| Event | Payload | Meaning |
|---|---|---|
| `UmbClipboardCopyRequestEvent` | item identity — `unique`, plus `name`/`icon` for presentation | "copy this item"; the host produces the value |
| `UmbClipboardPasteRequestEvent` | clipboard entry uniques | "paste these"; the host translates and places them |

Both are requests, not notifications: the receiver performs the operation. The input passes identities only, so it stays usable by editors with different value shapes.

Configuration travels the other way. The property editor derives what the input needs — whether copy is available, which entry value types to list, which entries are pastable — and hands it down as one object. Presence of a flag, not presence of a key, is the switch.

## Two paste routes, and they differ

Both go through the same translators, but not through the same code:

1. **The paste property action** (`kind: 'pasteFromClipboard'`) picks an entry and calls `setValue()` on the property context — it **replaces** the whole value, and never touches the property editor element.
2. **A picker's clipboard tab** returns entry uniques to the property editor, which translates and **merges** them into the existing value.

So logic placed in the property editor element applies to route 2 only. Anything that must hold for every paste belongs in the translator — that is what makes translators receiving config important.

## Translators receive the property's configuration

Both `translate()` and `isCompatibleValue()` are given the config of the property being pasted into, so a translator can produce a value that property editor actually supports — dropping options it isn't configured for rather than importing them blindly.

Config arrives as the **raw** `Array<UmbPropertyEditorConfigProperty>`, not a `UmbPropertyEditorConfigCollection`. Look values up by alias; `getValueByAlias` does not exist on it.

`isCompatibleValue()` is a gate, not a transform: it decides whether an entry is offered as pastable, and entries that fail render **disabled** rather than being hidden, so the user can see the entry exists and that only its compatibility is the problem.

## Pasting adds, it does not fit

A paste behaves like a pick: it adds what it was given, and validation reports a selection that is too long. It does not truncate to a configured maximum, and a single-value editor does not replace what it already holds. One entry can legitimately hold several items, and deciding which to keep is the user's, not a silent truncation that drops them without saying so.

## Rules

1. Gate copy affordances on copy availability and paste affordances on paste availability — never on "a clipboard context exists".
2. Never let an input component produce or consume a clipboard value. It reports an identity; the property editor owns the value.
3. Emit a lowest-common-denominator entry value alongside a rich one, so other editors can consume it.
4. Put anything that must hold for *every* paste in the translator, not the property editor element — the property action route bypasses the element.
5. Read config in a translator by alias off the raw array.
6. Let a paste exceed configured limits and leave it to validation; do not clamp or replace silently.
7. Name a clipboard entry value type for the *format*, never for the property editor that happens to produce it.

## Reference implementations

| Pattern | Location |
|---|---|
| Copy + paste, two entry value types, config-aware paste | `src/packages/media/media/property-editors/media-picker/clipboard/` |
| Paste only, from another editor's format | `src/packages/property-editors/content-picker/clipboard/` |
| Paste translator producing a composite value | `src/packages/tiptap/clipboard/` |
| Operating on behalf of nested items, naming their alias | `src/packages/block/block-list/context/` |
