import { defineConfig } from 'vite';
import type { OutputOptions } from 'rollup';
import path from 'node:path';
import { getDefaultConfig } from '../../src/vite-config-base';
import { unifiedManifestsPlugin } from './vite-plugin-unified-manifests';

const root = path.resolve(__dirname, '../..');
const dist = path.resolve(root, 'dist-cms/manifests-all');

const baseConfig = getDefaultConfig({ dist });
// getDefaultConfig always returns a single-object output (not an array); the
// cast tells TypeScript what its return contract guarantees.
const baseOutput = (baseConfig.build?.rollupOptions?.output ?? {}) as OutputOptions;

export default defineConfig({
	...baseConfig,
	build: {
		...baseConfig.build,
		copyPublicDir: false,
		// Vite 7 lib mode overrides rollupOptions.input; clear lib so the virtual entry is used instead.
		lib: undefined,
		rollupOptions: {
			...baseConfig.build?.rollupOptions,
			input: { index: 'virtual:unified-manifests' },
			// Required so Rollup keeps the named `allManifests` export on the entry chunk;
			// without this Rollup can elide re-exports when it judges them unused and
			// `import('.../manifests-all').then(mod => mod.allManifests)` returns undefined.
			preserveEntrySignatures: 'exports-only',
			output: {
				...baseOutput,
				entryFileNames: '[name].js',
				chunkFileNames: '[name]-[hash].js',
				format: 'es',
			},
		},
	},
	plugins: [
		unifiedManifestsPlugin({
			packagesRoot: path.resolve(root, 'src/packages'),
		}),
	],
});
