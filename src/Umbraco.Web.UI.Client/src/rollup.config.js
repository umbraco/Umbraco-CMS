import esbuild from 'rollup-plugin-esbuild';
import pluginJson from '@rollup/plugin-json';
import commonjs from '@rollup/plugin-commonjs';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import { readdirSync, lstatSync } from 'fs';

const readFolders = (path) => readdirSync(path).filter((folder) => lstatSync(`${path}/${folder}`).isDirectory());
const createModuleDescriptors = (folderName) =>
	readFolders(`./src/${folderName}`).map((moduleName) => {
		return {
			name: moduleName,
			file: `index.ts`,
			root: `./src/${folderName}/${moduleName}`,
			dist: `./dist-cms/${folderName}/${moduleName}`,
		};
	});

const exclude = ['css'];

const libs = createModuleDescriptors('libs');
const apps = createModuleDescriptors('apps');
const shared = createModuleDescriptors('shared');
const packages = createModuleDescriptors('packages');

// Packages are special, they can include multiple modules. We need to handle them differently.
// Modules are exposed as the umbraco-package.ts file in the root of the package. We can look through the exported module const to find the module names and src paths.
const packageSubModules = readFolders('./src/packages').map(async (packageName) => {
	const { modules } = await import(`./packages/${packageName}/modules.js`);

	const packageManifest = {
		name: packageName + '-package-manifest',
		file: `umbraco-package.ts`,
		root: `./src/packages/${packageName}`,
		dist: `./dist-cms/packages/${packageName}`,
	};

	const subModules = modules.map((module) => {
		return {
			name: packageName,
			file: `index.ts`,
			root: `./src/packages/${packageName}/${module.src}`,
			dist: `./dist-cms/packages/${packageName}/${module.src}`,
		};
	});

	return [packageManifest, ...subModules];
});

const something = await Promise.all(packageSubModules);
const modules = [...libs, ...apps, ...shared, ...packages, ...something].flat();
const allowedModules = modules.filter((module) => !exclude.includes(module.name));

export default allowedModules
	.map((module) => {
		/** @type {import('rollup').RollupOptions[]} */
		return [
			{
				input: `${module.root}/${module.file}`,
				external: [/^@umbraco-cms\//],
				output: {
					dir: `${module.dist}`,
					format: 'es',
					preserveModules: true,
					preserveModulesRoot: `${module.root}`,
				},
				plugins: [nodeResolve(), commonjs(), pluginJson(), esbuild()],
			},
		];
	})
	.flat();
