# Icon Manager

> **Experimental**: This tool is experimental and may change or be removed without notice.

A browser-based curation UI for the Umbraco backoffice icon dictionary
(`src/packages/core/icon-registry/icon-dictionary.json`). It loads the
dictionary, lets you edit metadata (keywords, groups, names), and exports a
new JSON blob to download.

The tool is its own self-contained Vite project — it has its own
`package.json`, lockfile, and `node_modules`. It only reads the parent
backoffice source tree; it does not write back automatically.

## Setup

```bash
npm install
```

## Usage

From this folder:

```bash
npm run dev
```

Or from the backoffice client root (`src/Umbraco.Web.UI.Client/`):

```bash
npm run dev:icon-manager
```

Both open the tool at `/icon-manager.html` in a browser.

## Applying changes

1. Edit icons in the UI.
2. Click **Export JSON** — the browser downloads an updated dictionary.
3. Replace `src/packages/core/icon-registry/icon-dictionary.json` with the
   downloaded file.
4. Re-run `npm run generate:icons` from the backoffice client root to
   regenerate the icon modules consumed at runtime.
