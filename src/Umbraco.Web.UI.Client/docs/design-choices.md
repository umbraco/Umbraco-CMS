# Design Choices
[← Umbraco Backoffice](../CLAUDE.md) | [← Monorepo Root](../../../CLAUDE.md)

---

UI stays behind. User content takes the visual front seat. Default to restraint on every visual decision.

When in doubt: leave the icon out, leave the colour off, cut the word.

This document covers visual design choices for the backoffice client (`src/Umbraco.Web.UI.Client/`). For naming and formatting conventions see [style-guide.md](./style-guide.md).

---

## Icons

Default: **no icon**. Add one only when the affordance cannot be carried by text — Entity Actions, toolbars without room for labels, drag handles, rich-text editor toolbars.

Don't decorate buttons, list rows, headings, or labels with icons for ornament. Don't pair an icon with a clear text label "for clarity" — pick one.

### Recognizable icons

When an icon *is* needed, use one of these. They're chosen because most users already recognize the symbol; reaching outside this list adds cognitive load.

**Action**
`icon-add` · `icon-trash` · `icon-edit` · `icon-search` · `icon-settings` · `icon-check` · `icon-undo` · `icon-clipboard-copy` · `icon-clipboard-paste` · `icon-link` · `icon-out` (sign out)

**Content type**
`icon-document` · `icon-folder` · `icon-picture` · `icon-user` · `icon-users` · `icon-globe` · `icon-cloud` · `icon-home`

**State**
`icon-lock` · `icon-unlocked` · `icon-alert` (warning) · `icon-time` (scheduled / pending) · `icon-circle-dotted` (generic fallback for entities without a specific icon)

**Navigation**
`icon-arrow-left` · `icon-arrow-up` · `icon-arrow-down`

**Drag handle**
`icon-grip`

**Rich-text editor toolbars only**
`icon-bold` · `icon-italic` · `icon-underline` · `icon-bulleted-list` · `icon-ordered-list` · `icon-blockquote` · `icon-code` · `icon-anchor`

### Looking for something else

If — and only if — none of the above fits and you're certain an icon is justified, browse `src/packages/core/icon-registry/icons/` for available aliases. The same set is exposed via `<umb-icon>` and the icon picker modal. Never invent an alias; if it isn't in the registry, use text.

For the upstream catalogue of UUI icons (used directly only for chrome like buttons), see the [UUI library](https://github.com/umbraco/Umbraco.UI).

---

## Colours

Default: **no colour**. UI chrome should be neutral — surface, border, body text. Reserve colour for the **single primary action** on a screen.

- No accent colours on info banners, hover effects beyond UUI defaults, or status pills for non-critical state.
- No hex codes, no raw colour names, no inventing CSS variables.
- Use UUI tokens (`--uui-color-…`) when colour is justified. The [UUI library](https://github.com/umbraco/Umbraco.UI) is the source of truth — don't enumerate tokens here, link there.

If you're reaching for colour to express hierarchy, the hierarchy probably belongs in layout, weight, or copy instead.

---

## Buttons

Only **one** button on screen has a non-default look. That's the primary call-to-action for the current view.

- Empty / placeholder views → the call-to-action is the primary.
- Everything else → secondary, or default.
- Don't repeat the primary look on multiple buttons to "balance the layout." Pick one.

### `look="placeholder"` — reserved usage

Use the placeholder look only when the button itself sits in the *visual destination* of its action — the button is a preview of where its result will land:

- "Add block" inside an empty grid slot
- "Add property" inside an empty schema row
- "Upload" inside an empty media drop zone

Don't use placeholder for buttons in toolbars, headers, dialogs, or anywhere disconnected from the result location.

---

## UX writing

Super short. Cut anything inferable from context.

- Use contextual reference: in the Members section, the button is "New", not "New member". On a delete confirm inside a Document workspace, "Delete" is enough.
- No marketing voice, no "please", no "in order to", no explanations.
- One verb beats two. One sentence beats two.

### Before / after

| Don't | Do |
|-------|----|
| "Create a new member" | "New" |
| "Please click here to upload your image" | "Upload" |
| "Search for content here…" | "Search" |
| "Successfully saved your changes" | "Saved" |

---

## Dialogs

A dialog already implies confirmation — don't say "are you sure". Sharp, concrete copy. Two options: do, or cancel.

Source: [UUI Dialog Style Guide](https://uui.umbraco.com/?path=/story/design-style-guide--dialog).

### Headline

The action in short form. Include the target only if it sits naturally at the end.

| Don't | Do |
|-------|----|
| "Confirm deleting 'My Page 1'" | "Delete 'My Page 1'" |
| "Publish 'My Page 1' with descendants" | "Publish with descendants" |
| "Transfer document" | "Transfer to 'Development'" |

### Description

Clarify the *effect* of the action without repeating the headline. Don't assume the user knows internal vocabulary — unfold words like "publish" or "descendants" with their concrete consequence.

Use `<strong>` for target names (easy to anchor on visually); use `<i>` for secondary effects.

| Don't | Do |
|-------|----|
| "Are you sure you want to delete this content and all content items underneath?" | "<strong>My Page 1</strong> will be transferred to the trash bin, including <i>all content items underneath</i>." |
| "Are you sure you want to publish My Page 1 and all descendant items." | "Make <strong>My Page 1</strong> publicly available, including <i>all content items underneath</i>." |

### Actions

Two buttons: cancel (default look) and the action (primary, `color="positive"` or `color="danger"`). The button label is just the verb — `Delete`, `Publish`, `Transfer` — matching the word used in the headline or description so the eye connects them.

Never put the target name in the action button. The text carries the target; the button carries the verb.

---

## Layout & spacing

Use UUI spacing tokens (`--uui-size-*`), prefer (`--uui-size-space-*`), not hardcoded pixels. The system is built on a 6px base-unit; tokens compose into a vertical rhythm that keeps the UI calm.

The space between elements declares their relationship — related elements closer, unrelated elements farther. If no token fits, rethink the layout instead of inventing a value.
Optical adjustments are welcome, but need careful consideration before being introduced.

Source: [UUI Layout Style Guide](https://uui.umbraco.com/?path=/story/design-style-guide--layout).

---

## Typography

Use UUI's type system. Don't invent font sizes.

- Apply `.uui-text` to the root of a UUI typography region.
- Headings: native `<h1>`–`<h5>` or the equivalent `.uui-h1`–`.uui-h5` classes.
- Lead paragraph: `<p class="uui-lead">` for a summary opener.
- `<small>` for inline descriptions.

Hierarchy comes from size and weight — never colour.

Source: [UUI Typography Style Guide](https://uui.umbraco.com/?path=/story/design-style-guide--typography).
