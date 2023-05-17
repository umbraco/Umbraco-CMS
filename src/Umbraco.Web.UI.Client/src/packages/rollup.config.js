import esbuild from 'rollup-plugin-esbuild';
import pluginJson from '@rollup/plugin-json';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import { readdirSync, lstatSync } from 'fs';

const exclude = [];

const corePackages = readdirSync('./src/packages').filter((corePackage) =>
	lstatSync(`src/packages/${corePackage}`).isDirectory()
);

const allowedPackages = corePackages.filter((corePackage) => !exclude.includes(corePackage));

export default allowedPackages
	.map((corePackage) => {
		/** @type {import('rollup').RollupOptions[]} */
		return [
			{
				input: `./src/packages/${corePackage}/index.ts`,
				external: [/^@umbraco-cms\//],
				output: {
					dir: `./src/packages/${corePackage}/dist`,
					format: 'es',
					preserveModules: true,
					preserveModulesRoot: `./src/packages/${corePackage}`,
				},
				plugins: [nodeResolve(), pluginJson(), esbuild()],
			},
		];
	})
	.flat();
