import { defineConfig } from 'vite';
import path from 'node:path';
import { getDefaultConfig } from '../../src/vite-config-base';
import { unifiedManifestsPlugin } from './vite-plugin-unified-manifests';

const root = path.resolve(__dirname, '../..');
const dist = path.resolve(root, 'dist-cms/manifests-all');

const baseConfig = getDefaultConfig({ dist });

const baseOutput = Array.isArray(baseConfig.build?.rollupOptions?.output)
	? {}
	: (baseConfig.build?.rollupOptions?.output ?? {});

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
