import esbuild from 'rollup-plugin-esbuild';
import commonjs from '@rollup/plugin-commonjs';
import { nodeResolve } from '@rollup/plugin-node-resolve';

export default [
	{
		input: './external/backend-api/index.ts',
		external: [],
		output: {
			dir: `./dist-cms/external/backend-api`,
			format: 'es',
			preserveModules: true,
			preserveModulesRoot: `./external/backend-api`,
		},
		plugins: [nodeResolve(), commonjs(), esbuild()],
	},
	{
		input: './external/openid/index.ts',
		output: {
			dir: `./dist-cms/external/openid`,
			format: 'es',
		},
		plugins: [nodeResolve(), commonjs(), esbuild()],
	},
	{
		input: './external/rxjs/index.ts',
		output: {
			dir: `./dist-cms/external/rxjs`,
			format: 'es',
		},
		plugins: [nodeResolve(), commonjs(), esbuild()],
	},
	{
		input: './external/router-slot/index.ts',
		output: {
			dir: `./dist-cms/external/router-slot`,
			format: 'es',
		},
		plugins: [nodeResolve(), commonjs(), esbuild()],
	},
	{
		input: './external/uuid/index.ts',
		output: {
			dir: `./dist-cms/external/uuid`,
			format: 'es',
		},
		plugins: [nodeResolve(), commonjs(), esbuild()],
	},
	{
		input: './external/lit/index.ts',
		output: {
			dir: `./dist-cms/external/lit`,
			format: 'es',
		},
		plugins: [nodeResolve(), commonjs(), esbuild()],
	},
];
