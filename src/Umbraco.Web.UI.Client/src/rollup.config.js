import esbuild from 'rollup-plugin-esbuild';
import commonjs from '@rollup/plugin-commonjs';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import css from 'rollup-plugin-import-css';
import webWorkerLoader from 'rollup-plugin-web-worker-loader';
import { readdirSync, lstatSync, rmSync, cpSync, copyFileSync } from 'fs';

/* TODO Temp solution. I can't find a way for rollup to overwrite the external folder that is already created
by tsc. So I'm deleting it before the build.*/
console.log('--- Deleting existing external folder ---');
rmSync('./dist-cms/external', { recursive: true, force: true });
console.log('--- Deleting existing external done ---');

// Copy assets
console.log('--- Copying assets ---');
cpSync('./src/assets', './dist-cms/assets', { recursive: true });
console.log('--- Copying assets done ---');

// Copy SRC CSS
console.log('--- Copying src CSS ---');
cpSync('./src/css', './dist-cms/css', { recursive: true });
console.log('--- Copying src CSS done ---');

// Copy UUI CSS
console.log('--- Copying UUI CSS ---');
copyFileSync('./node_modules/@umbraco-ui/uui-css/dist/uui-css.css', './dist-cms/css/uui-css.css');
console.log('--- Copying src UUI CSS done ---');

// Copy UUI FONTS
console.log('--- Copying UUI Fonts ---');
cpSync('./node_modules/@umbraco-ui/uui-css/assets/fonts', './dist-cms/assets/fonts', { recursive: true });
console.log('--- Copying src UUI Fonts done ---');

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
const libraries = allowed
	.map((module) => {
		/** @type {import('rollup').RollupOptions} */
		return {
			input: `./src/external/${module.name}/index.ts`,
			output: {
				dir: `./dist-cms/external/${module.name}`,
				format: 'es',
			},
			plugins: [nodeResolve(), webWorkerLoader({ target: 'browser', pattern: /^(.+)\?worker$/ }), commonjs(), css(), esbuild({ minify: true, sourceMap: true })],
		}
	});

/** @type {import('rollup').RollupOptions[]} */
export default [
	...libraries,
]

