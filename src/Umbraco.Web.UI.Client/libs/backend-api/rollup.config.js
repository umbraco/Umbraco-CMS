import esbuild from 'rollup-plugin-esbuild';
//import { nodeResolve } from '@rollup/plugin-node-resolve';

export default [
	{
		input: 'index.ts',
		output: {
			file: 'dist/index.js',
			format: 'es',
		},
		plugins: [esbuild({ sourceMap: true })],
	},
];
