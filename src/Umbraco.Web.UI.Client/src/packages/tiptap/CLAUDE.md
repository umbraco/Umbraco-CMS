# Umbraco Tiptap package - @umbraco-cms/backoffice/tiptap

[Umbraco Backoffice](../../../CLAUDE.md) | [Umbraco CMS Root](../../../../../CLAUDE.md)

---

## Overview

Extensible rich text editor (RTE) framework for the Umbraco CMS backoffice. Built on **Tiptap v3** (based on ProseMirror), this package provides a plugin architecture with 50+ built-in extensions for content editing.

**Package**: `@umbraco-cms/backoffice/tiptap`
**Name**: `Umbraco.Core.Tiptap`

### External Resources

- **Tiptap**: [tiptap.dev](https://tiptap.dev/) | [Documentation](https://tiptap.dev/docs) | [Source](https://github.com/ueberdosis/tiptap/)
- **ProseMirror** (underlying framework): [prosemirror.net](https://prosemirror.net/) | [Documentation](https://prosemirror.net/docs/) | [Source](https://github.com/ProseMirror/prosemirror)

---

## Directory Structure

```
tiptap/
├── components/                            # UI components
│   ├── input-tiptap/                      # Main editor component (`umb-input-tiptap`)
│   ├── toolbar/                           # Editor toolbar (`umb-tiptap-toolbar`)
│   ├── statusbar/                         # Editor status bar (`umb-tiptap-statusbar`)
│   ├── menu/                              # Menu system (`umb-tiptap-menu`)
│   └── cascading-menu-popover/            # Nested menu UI (`umb-cascading-menu-popover`)
├── contexts/                              # Context API
│   └── tiptap-rte.context.ts              # Shared editor context (`UMB_TIPTAP_RTE_CONTEXT`)
├── extensions/                            # 50+ editor extensions
│   ├── tiptap-extension-api-base.ts       # Base class for extensions
│   ├── tiptap-toolbar-element-api-base.ts # Base class for toolbar actions
│   ├── types.ts                           # API interfaces
│   ├── manifests.ts                       # Aggregated extension manifests
│   └── [extension-name]/                  # Individual extensions
│       ├── {name}.tiptap-api.ts           # Extension API
│       ├── {name}.tiptap-extension.ts     # Custom Tiptap extension (if needed)
│       ├── {name}.tiptap-statusbar-api.ts # Statusbar action API
│       ├── {name}.tiptap-toolbar-api.ts   # Toolbar action API
│       └── manifests.ts                   # Extension manifests
├── property-editors/                      # Property editor configurations
│   ├── tiptap-rte/                        # Main RTE property editor UI
│   ├── extensions-configuration/          # Capabilities/extension picker
│   ├── toolbar-configuration/             # Toolbar layout builder
│   └── statusbar-configuration/           # Statusbar layout builder
├── constants.ts                           # Aggregated exported constants
├── externals.ts                           # Tiptap library re-exports
├── index.ts                               # Aggregated exports
├── manifests.ts                           # Root manifest registry
├── types.ts                               # Aggregated type exports
└── umbraco-package.ts                     # Package metadata
```

---

## Extension Architecture

### Extension Types

| Type                    | Manifest Type              | Purpose                                               |
| ----------------------- | -------------------------- | ----------------------------------------------------- |
| **Tiptap Extension**    | `tiptapExtension`          | Registers editor capabilities (marks, nodes, plugins) |
| **Toolbar Extension**   | `tiptapToolbarExtension`   | UI actions in the toolbar                             |
| **Statusbar Extension** | `tiptapStatusbarExtension` | Status/metadata display                               |

### Base Classes

**`UmbTiptapExtensionApiBase`** - For editor extensions:

```typescript
export default class UmbTiptapMyExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [MyTiptapExtension];

	// Optional: provide custom CSS for the extension
	override getStyles = () => css`
		/* styles */
	`;
}
```

**`UmbTiptapToolbarElementApiBase`** - For toolbar actions:

```typescript
export default class UmbTiptapMyToolbarApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBold().run();
	}

	// Optional: override active/disabled state
	override isActive(editor?: Editor): boolean { ... }
	override isDisabled(editor?: Editor): boolean { ... }
}
```

### Toolbar Button Kinds

Four built-in kinds for toolbar extensions:

- `button` - Standard toolbar button
- `colorPickerButton` - Color picker button
- `menu` - Dropdown menu
- `styleMenu` - Style selector dropdown

---

## File Naming Conventions

| File Pattern                         | Purpose                     |
| ------------------------------------ | --------------------------- |
| `{name}.tiptap-api.ts`               | Extension API class         |
| `{name}.tiptap-extension.ts`         | Custom Tiptap extension     |
| `{name}.tiptap-toolbar-api.ts`       | Toolbar action API          |
| `{name}.tiptap-toolbar-element.ts`   | Toolbar UI element          |
| `{name}.tiptap-statusbar-api.ts`     | Statusbar action API        |
| `{name}.tiptap-statusbar-element.ts` | Statusbar UI element        |
| `manifests.ts`                       | Extension manifest registry |

---

## Creating Extensions

### Simple Extension (e.g., Bold)

```typescript
// bold.tiptap-api.ts
import { Bold } from '../../externals.js';
import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';

export default class UmbTiptapBoldExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Bold];
}

// bold.tiptap-toolbar-api.ts
import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import type { Editor } from '../../externals.js';

export default class UmbTiptapBoldToolbarApi extends UmbTiptapToolbarElementApiBase {
	override execute(editor?: Editor) {
		editor?.chain().focus().toggleBold().run();
	}
}

// manifests.ts
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'tiptapExtension',
		alias: 'Umb.Tiptap.Bold',
		name: 'Bold Tiptap Extension',
		api: () => import('./bold.tiptap-api.js'),
		meta: {
			icon: 'icon-bold',
			label: 'Bold',
			group: '#tiptap_extGroup_formatting',
		},
	},
	{
		type: 'tiptapToolbarExtension',
		kind: 'button',
		alias: 'Umb.Tiptap.Toolbar.Bold',
		name: 'Bold Tiptap Toolbar Extension',
		api: () => import('./bold.tiptap-toolbar-api.js'),
		forExtensions: ['Umb.Tiptap.Bold'],
		meta: {
			alias: 'bold',
			icon: 'icon-bold',
			label: '#buttons_bold',
		},
	},
];
```

### Extension with Custom Styles

Override `getStyles()` to inject CSS into the editor's Shadow DOM:

```typescript
export default class UmbTiptapTableExtensionApi extends UmbTiptapExtensionApiBase {
	getTiptapExtensions = () => [Table, TableRow, TableHeader, TableCell];

	override getStyles = () => css`
		table {
			border-collapse: collapse;
			width: 100%;
		}
		td,
		th {
			border: 1px solid var(--uui-color-border);
			padding: 0.5rem;
		}
	`;
}
```

### Extension with Modal Dialog

Use `UMB_MODAL_MANAGER_CONTEXT` for dialogs:

```typescript
export default class UmbTiptapLinkToolbarApi extends UmbTiptapToolbarElementApiBase {
	override async execute(editor?: Editor) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modal = modalManager.open(this, UMB_LINK_PICKER_MODAL, { data: { ... } });
		const result = await modal.onSubmit();
		editor?.chain().setLink({ href: result.url }).run();
	}
}
```

---

## Key Components

### Main Editor (`umb-input-tiptap`)

The primary editor component with:

- Dynamic extension loading based on configuration
- Toolbar and statusbar rendering
- Shadow DOM scoping with custom stylesheets
- Form control with validation support
- Read-only mode

**Properties**:

- `value: string` - HTML content
- `configuration: UmbPropertyEditorConfigCollection` - Editor configuration
- `label: string` - Accessibility label
- `readonly: boolean` - Read-only mode
- `required: boolean` - Validation flag

### Editor Context (`UMB_TIPTAP_RTE_CONTEXT`)

Shares the Tiptap Editor instance across the component tree:

```typescript
this.consumeContext(UMB_TIPTAP_RTE_CONTEXT, (context) => {
	const editor = context.getEditor();
});
```

---

## Configuration

The editor is configured via `UmbPropertyEditorConfigCollection`:

| Key           | Type                      | Description                                  |
| ------------- | ------------------------- | -------------------------------------------- |
| `extensions`  | `Array<string>`           | Enabled extension aliases                    |
| `toolbar`     | `UmbTiptapToolbarValue`   | Toolbar layout (3D array: rows/groups/items) |
| `statusbar`   | `UmbTiptapStatusbarValue` | Statusbar layout (2D array: sections/items)  |
| `stylesheets` | `Array<string>`           | Custom CSS URLs                              |

---

## Important Notes

### Core Extension

`Umb.Tiptap.RichTextEssentials` is always enabled and provides:

- Document, Paragraph, Text, HardBreak
- Dropcursor, Gapcursor
- Undo/Redo

### Tiptap Library Imports

Always import Tiptap types and extensions from `externals.ts`:

```typescript
// Correct
import { Bold, Editor } from '../../externals.js';
import type { Extension } from '../../externals.js';

// Incorrect - bypasses project configuration
import { Bold } from '@tiptap/extension-bold';
```

### Shadow DOM Scoping

Custom CSS is injected via `<style>` tags (not Tiptap's `injectCSS` option) to work with Shadow DOM encapsulation.

### Debounced Updates

Toolbar and statusbar re-renders are debounced at 100ms to prevent excessive updates during rapid editing.

---

## Quick Reference

| Item                 | Path/Value                                        |
| -------------------- | ------------------------------------------------- |
| Main component       | `components/input-tiptap/input-tiptap.element.ts` |
| Extension base class | `extensions/tiptap-extension-api-base.ts`         |
| Toolbar base class   | `extensions/tiptap-toolbar-element-api-base.ts`   |
| Context token        | `UMB_TIPTAP_RTE_CONTEXT`                          |
| Tiptap imports       | `externals.ts`                                    |
| All manifests        | `extensions/manifests.ts`                         |
| Bundle alias         | `Umb.Bundle.Tiptap`                               |
