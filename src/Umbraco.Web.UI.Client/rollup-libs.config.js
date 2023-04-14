import esbuild from 'rollup-plugin-esbuild';
import pluginJson from '@rollup/plugin-json';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import dts from 'rollup-plugin-dts';
import { readdirSync, lstatSync } from 'fs';

const libs = readdirSync('./libs').filter(lib => lstatSync(`libs/${lib}`).isDirectory());
const outputDir = './dist/libs';

export default libs.map(lib => {
	/** @type {import('rollup').RollupOptions[]} */
	return [
		{
			input: `./libs/${lib}/index.ts`,
			external: [/^@umbraco-cms\//],
			output: {
				file: `${outputDir}/${lib}.js`,
				format: 'es',
				sourcemap: true,
			},
			plugins: [nodeResolve(), pluginJson(), esbuild()]
		},
		{
			input: `./libs/${lib}/index.ts`,
			external: [/^@umbraco/, /^lit/, /^rxjs/, /^uuid/],
			output: {
				file: `${outputDir}/${lib}.d.ts`,
				format: 'es'
			},
			plugins: [dts({ respectExternal: true })],
		}
	];
}).flat();
