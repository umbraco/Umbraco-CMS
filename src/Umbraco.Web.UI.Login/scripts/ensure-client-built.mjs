import { existsSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const clientDir = resolve(here, '../../Umbraco.Web.UI.Client');
const distCms = resolve(clientDir, 'dist-cms');

if (!existsSync(distCms)) {
	console.error(`
╭──────────────────────────────────────────────────────────────────────╮
│ ERROR: Umbraco.Web.UI.Client has not been built.                     │
│                                                                      │
│ Login consumes the sibling Client project for types via a            │
│ \`file:\` dependency. The Client's \`dist-cms/\` output must exist       │
│ before Login can be type-checked or built.                           │
│                                                                      │
│ Run this first:                                                      │
│                                                                      │
│   cd ${clientDir}
│   npm install                                                        │
│   npm run build:for:cms                                              │
│                                                                      │
│ Then retry the Login command.                                        │
╰──────────────────────────────────────────────────────────────────────╯
`);
	process.exit(1);
}
