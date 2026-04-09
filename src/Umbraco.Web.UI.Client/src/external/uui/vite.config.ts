import { defineConfig } from 'vite';
import { cpSync, rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/external/uui';
const distAssets = '../../../dist-cms';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

// copy css
cpSync('../../../node_modules/@umbraco-ui/uui-css/dist/uui-css.css', `${distAssets}/css/uui-css.css`, {
	recursive: true,
});

// copy fonts
cpSync('../../../node_modules/@umbraco-ui/uui-css/assets/fonts', `${distAssets}/assets/fonts`, {
	recursive: true,
});

export default defineConfig({
	...getDefaultConfig({
		dist,
		base: '/umbraco/backoffice/external/uui',
		entry: {
			index: './index.ts',
		},
	}),
});
