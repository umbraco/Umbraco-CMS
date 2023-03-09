import esbuild from 'rollup-plugin-esbuild';
import pluginJson from '@rollup/plugin-json';
import { nodeResolve } from '@rollup/plugin-node-resolve';

/** @type {import('rollup').RollupOptions} */
export default {
	input: 'index.ts',
	external: [/^@umbraco-cms\//],
	output: {
		file: 'dist/index.js',
		format: 'es',
		sourcemap: true,
	},
	plugins: [nodeResolve(), pluginJson(), esbuild({ sourceMap: true })],
};
