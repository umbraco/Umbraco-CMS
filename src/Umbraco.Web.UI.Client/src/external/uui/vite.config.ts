import { defineConfig } from 'vite';
import { cpSync, rmSync } from 'fs';
import { getDefaultConfig } from '../../vite-config-base';

const dist = '../../../dist-cms/external/uui';
const distAssets = '../../../dist-cms';

// delete the unbundled dist folder
rmSync(dist, { recursive: true, force: true });

// Copy UUI theme CSS (dark.css, high-contrast.css, etc.) into dist-cms/css/.
// These are served at /umbraco/backoffice/css/ and referenced by theme manifests
// in src/packages/core/themes/manifests.ts. If UUI changes its theme filenames,
// update the manifests to match.
cpSync('../../../node_modules/@umbraco-ui/uui/dist/themes', `${distAssets}/css`, {
	recursive: true,
});

// copy fonts
cpSync('../../../node_modules/@umbraco-ui/uui/dist/assets/fonts', `${distAssets}/assets/fonts`, {
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
