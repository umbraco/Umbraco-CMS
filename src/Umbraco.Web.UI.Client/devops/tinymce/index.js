import * as fs from 'fs/promises';
import * as pathModule from 'path';
import * as globModule from 'tiny-glob';
import uglify from 'uglify-js';
import * as cleanCssModule from 'clean-css';

/**
 * What's going on here? TinyMCE is a fickle beast and needs a very particular setup to work nicely
 * in a modern js environment. To make that work, this module copies the required assets (ie css, plugins, themes etc)
 * from node_modules to public, so that the files are available from the same location as they
 * will be in the published application. This is required because TinyMCE configuration requires a base path otherwise
 * it attempts to import dependencies relative to the executing component, which fails miserably.
 * 
 * So, to make it work, here we copy the required assets, and rename to .min. as the tinyMCE configuration
 * also specifies using the minified assets. However (another however) not all assets have minified versions in the npm
 * package - some exist, some don't exist at all, others exist with no content. To work around that, non-minified
 * files are minified, renamed and copied to the destination folder.
 * 
 * Finally, when the application is built, the public assets are copied as-is to /dist/tinymce
 */

const glob = globModule.default;
const path = pathModule.default;
const cleanCss = cleanCssModule.default;

const getDirName = path.dirname;
const destDir = 'public/tinymce';

const tinyAssets = {
	source: 'node_modules/tinymce',
	globs: [
		'/license.txt',
		'/icons/default/icons.min.js',
		'/models/dom/model.min.js',
		'/plugins/**/plugin.js',
		'/skins/ui/**/content.css',
		'/skins/ui/**/content.inline.css',
		'/skins/ui/**/skin.css',
		'/skins/ui/**/skin.shadowdom.css',
		'/skins/content/**/content.css',
		'/themes/silver/theme.min.js',
	],
}

const i18nAssets = {
	source: 'node_modules/tinymce-i18n',
	globs: [
		'/license',
		'/langs6/**/*.js',
	],
	pathMaps: [
		{from: 'langs6', to: 'langs'},
	],
}

const min = (path) => {
	const parts = path.split('.');
	parts[parts.length - 1] = `min.${parts[parts.length - 1]}`;
	return parts.join('.');
};

const copyTinyMceAssets = async (sourceDir, globPath, pathMaps = [], minify = true) => {
	const filesToCopy = await glob(`${sourceDir}${globPath}`);
	for (let file of filesToCopy) {
		file = file.replace(/\\/g, '/');

		let to = file.replace(sourceDir, destDir);
		pathMaps.forEach(map => {
			to = to.replace(map.from, map.to);
		});

		await fs.mkdir(getDirName(to), { recursive: true });

		if (!minify || file.includes('.min.') || file.includes('.txt')) {
			await fs.copyFile(file, to);
		} else {
            const content = await fs.readFile(file, 'utf8');
            if (file.endsWith('js')) {
                const minified = uglify.minify(content);
                await fs.writeFile(min(to), minified.code);
            } else if (file.endsWith('css')) {
                const minified = new cleanCss().minify(content);
                await fs.writeFile(min(to), minified.styles);
            }
		}
	}
};

const run = async () => {
	await fs.rm(destDir, { recursive: true, force: true });

	for (const glob of tinyAssets.globs) {
		await copyTinyMceAssets(tinyAssets.source, glob);
	}

	for (const glob of i18nAssets.globs) {
		await copyTinyMceAssets(i18nAssets.source, glob, i18nAssets.pathMaps, false);
	}
};

run();
