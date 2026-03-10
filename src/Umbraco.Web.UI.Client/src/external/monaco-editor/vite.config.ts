import { defineConfig } from 'vite';
import { cpSync, rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/external/monaco-editor';
const distAssets = '../../../dist-cms';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

// copy fonts
cpSync('../../../node_modules/monaco-editor/esm/vs/base/browser/ui/codicons', `${distAssets}/assets/fonts`, {
	recursive: true,
});

export default defineConfig({
	...getDefaultConfig({
		dist,
		base: '/umbraco/backoffice/external/monaco-editor',
		entry: {
			index: './index.ts',
		},
	}),
});
