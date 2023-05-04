import esbuild from 'rollup-plugin-esbuild';
import pluginJson from '@rollup/plugin-json';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import { readdirSync, lstatSync } from 'fs';

const exclude = ['core', 'users'];

const corePackages = readdirSync('./src/backoffice').filter((corePackage) =>
	lstatSync(`src/backoffice/${corePackage}`).isDirectory()
);

const allowedPackages = corePackages.filter((corePackage) => !exclude.includes(corePackage));

export default allowedPackages
	.map((corePackage) => {
		/** @type {import('rollup').RollupOptions[]} */
		return [
			{
				input: `./src/backoffice/${corePackage}/index.ts`,
				external: [/^@umbraco-cms\//],
				output: {
					dir: `./src/backoffice/${corePackage}/dist`,
					format: 'es',
					preserveModules: true,
					preserveModulesRoot: `./src/backoffice/${corePackage}`,
				},
				plugins: [nodeResolve(), pluginJson(), esbuild()],
			},
		];
	})
	.flat();
