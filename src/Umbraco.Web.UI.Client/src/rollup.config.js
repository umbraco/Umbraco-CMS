import esbuild from 'rollup-plugin-esbuild';
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

const externals = createModuleDescriptors('external');
const exclude = [];
const allowed = externals.filter((module) => !exclude.includes(module.name));

// TODO: Minify code
export default allowed
	.map((module) => {
		/** @type {import('rollup').RollupOptions[]} */
		return [
			{
				input: `./src/external/${module.name}/index.ts`,
				minify: true,
				output: {
					dir: `./dist-cms/external/${module.name}`,
					format: 'es',
				},
				plugins: [nodeResolve(), commonjs(), esbuild()],
			},
		];
	})
	.flat();
