import esbuild from 'rollup-plugin-esbuild';
import pluginJson from '@rollup/plugin-json';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import dts from 'rollup-plugin-dts';

/** @type {import('rollup').RollupOptions[]} */
export default [
	{
		input: 'index.ts',
		external: [/^@umbraco-cms\//, /^lit/],
		output: {
			file: 'dist/index.js',
			format: 'es',
			sourcemap: true
		},
		plugins: [nodeResolve(), pluginJson(), esbuild()]
	},
	{
		input: 'index.ts',
		external: [/^@umbraco-cms\//, /^lit/, /^rxjs/],
		output: {
			file: './dist/index.d.ts',
			format: 'es'
		},
		plugins: [dts({ respectExternal: true })],
	}
];
