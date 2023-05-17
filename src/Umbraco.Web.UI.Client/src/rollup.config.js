import esbuild from 'rollup-plugin-esbuild';
import pluginJson from '@rollup/plugin-json';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import { readdirSync, lstatSync } from 'fs';

const readFolders = (path) => readdirSync(path).filter((module) => lstatSync(`${path}/${module}`).isDirectory());
const createModuleDescriptors = (folderName) =>
	readFolders(`./src/${folderName}`).map((moduleName) => {
		return {
			name: moduleName,
			root: `./src/${folderName}/${moduleName}`,
			dist: `./dist-cms/${folderName}/${moduleName}`,
		};
	});

const exclude = ['app'];
const packages = createModuleDescriptors('packages');
const apps = createModuleDescriptors('apps');

const modules = [...apps, ...packages];
const allowedModules = modules.filter((module) => !exclude.includes(module.name));

console.log(allowedModules);

export default allowedModules
	.map((module) => {
		/** @type {import('rollup').RollupOptions[]} */
		return [
			{
				input: `${module.root}/index.ts`,
				external: [/^@umbraco-cms\//],
				output: {
					dir: `${module.dist}`,
					format: 'es',
					preserveModules: true,
					preserveModulesRoot: `${module.root}`,
				},
				plugins: [nodeResolve(), pluginJson(), esbuild()],
			},
		];
	})
	.flat();
