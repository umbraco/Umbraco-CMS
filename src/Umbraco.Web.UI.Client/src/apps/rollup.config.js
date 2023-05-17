import esbuild from 'rollup-plugin-esbuild';
import pluginJson from '@rollup/plugin-json';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import { readdirSync, lstatSync } from 'fs';

const exclude = [];
const apps = readdirSync('./src/apps').filter((corePackage) => lstatSync(`src/apps/${corePackage}`).isDirectory());
const allowedApps = apps.filter((corePackage) => !exclude.includes(corePackage));

export default allowedApps
	.map((app) => {
		/** @type {import('rollup').RollupOptions[]} */
		return [
			{
				input: `./src/apps/${app}/index.ts`,
				external: [/^@umbraco-cms\//],
				output: {
					dir: `./src/apps/${app}/dist`,
					format: 'es',
					preserveModules: true,
					preserveModulesRoot: `./src/apps/${app}`,
				},
				plugins: [nodeResolve(), pluginJson(), esbuild()],
			},
		];
	})
	.flat();
