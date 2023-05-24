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
];
