import esbuild from 'rollup-plugin-esbuild';
import { nodeResolve } from '@rollup/plugin-node-resolve';
import css from 'rollup-plugin-import-css';
import replace from '@rollup/plugin-replace';
import { readdirSync, lstatSync, cpSync, copyFileSync, existsSync, unlinkSync } from 'fs';
import * as globModule from 'tiny-glob';

const glob = globModule.default;

// TODO: could we rename this to just dist?
const DIST_DIRECTORY = './dist-cms';

/* TODO: temp solution. Not every external library can run in the browser so we need rollup to bundle them and make them Browser friendly.
For each external library we want to bundle all its files into one js bundle. First we run the
Typescript compiler to create the external folder with d.ts files in the correct places. Then we delete all the js modules that are created, but
might not work in the browser. Then we run rollup to bundle the external libraries. */
console.log('--- Deleting temp external JS modules ---');
const tempJsFiles = await glob(`${DIST_DIRECTORY}/external/**/*.js`);
tempJsFiles.forEach((path) => existsSync(path) && unlinkSync(path));
console.log('--- Deleting temp external JS modules done ---');

// Copy assets
console.log('--- Copying assets ---');
cpSync('./src/assets', `${DIST_DIRECTORY}/assets`, { recursive: true });
console.log('--- Copying assets done ---');

// Copy SRC CSS
console.log('--- Copying src CSS ---');
cpSync('./src/css', `${DIST_DIRECTORY}/css`, { recursive: true });
console.log('--- Copying src CSS done ---');

// Copy UUI CSS
console.log('--- Copying UUI CSS ---');
copyFileSync('./node_modules/@umbraco-ui/uui-css/dist/uui-css.css', `${DIST_DIRECTORY}/css/uui-css.css`);
console.log('--- Copying src UUI CSS done ---');

// Copy UUI FONTS
console.log('--- Copying UUI Fonts ---');
cpSync('./node_modules/@umbraco-ui/uui-css/assets/fonts', `${DIST_DIRECTORY}/assets/fonts`, { recursive: true });
console.log('--- Copying src UUI Fonts done ---');

// Copy TinyMCE
console.log('--- Copying TinyMCE ---');
cpSync('./node_modules/tinymce', `${DIST_DIRECTORY}/tinymce`, { recursive: true });
console.log('--- Copying TinyMCE done ---');

// Copy TinyMCE i18n
console.log('--- Copying TinyMCE i18n ---');
cpSync('./node_modules/tinymce-i18n/langs6', `${DIST_DIRECTORY}/tinymce/langs`, { recursive: true });
console.log('--- Copying TinyMCE i18n done ---');

// Copy monaco-editor
console.log('--- Copying monaco-editor ---');
cpSync('./node_modules/monaco-editor/esm/vs/editor/editor.worker.js', `${DIST_DIRECTORY}/monaco-editor/vs/editor/editor.worker.js`);
cpSync('./node_modules/monaco-editor/esm/vs/base', `${DIST_DIRECTORY}/monaco-editor/vs/base`, { recursive: true });
cpSync('./node_modules/monaco-editor/esm/vs/nls.js', `${DIST_DIRECTORY}/monaco-editor/vs/nls.js`, { recursive: true });
cpSync('./node_modules/monaco-editor/esm/vs/nls.messages.js', `${DIST_DIRECTORY}/monaco-editor/vs/nls.messages.js`, { recursive: true });
cpSync('./node_modules/monaco-editor/esm/vs/editor/common', `${DIST_DIRECTORY}/monaco-editor/vs/editor/common`, { recursive: true });
cpSync('./node_modules/monaco-editor/esm/vs/language', `${DIST_DIRECTORY}/monaco-editor/vs/language`, { recursive: true });
cpSync('./node_modules/monaco-editor/min/vs/base/browser/ui/codicons', `${DIST_DIRECTORY}/assets/fonts`, { recursive: true });
console.log('--- Copying monaco-editor done ---');

const readFolders = (path) => readdirSync(path).filter((folder) => lstatSync(`${path}/${folder}`).isDirectory());
const createModuleDescriptors = (folderName) =>
	readFolders(`./src/${folderName}`).map((moduleName) => {
		return {
			name: moduleName,
			file: `index.ts`,
			root: `./src/${folderName}/${moduleName}`,
			dist: `${DIST_DIRECTORY}/${folderName}/${moduleName}`,
		};
	});

const externals = createModuleDescriptors('external');
const exclude = [];
const allowed = externals.filter((module) => !exclude.includes(module.name));

// TODO: Minify code
const libraries = allowed.map((module) => {
	/** @type {import('rollup').RollupOptions} */
	return {
		input: `./src/external/${module.name}/index.ts`,
		output: {
			dir: `${DIST_DIRECTORY}/external/${module.name}`,
			format: 'es',
		},
		plugins: [
			nodeResolve({ preferBuiltins: false, browser: true }),
			// Replace the vite specific inline query with nothing so that the import is valid
			replace({
				preventAssignment: true,
				values: {
					'?inline': '',
				},
			}),
			css({ minify: true }),
			esbuild({ minify: true, sourceMap: true }),
		],
	};
});

/** @type {import('rollup').RollupOptions[]} */
export default [...libraries];
