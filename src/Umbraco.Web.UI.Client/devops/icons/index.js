import { readFileSync, writeFile, mkdir } from 'fs';
import * as globModule from 'tiny-glob';
import * as pathModule from 'path';

const path = pathModule.default;
const getDirName = path.dirname;
const glob = globModule.default;

const moduleDirectory = 'src/shared/icon-registry';
const iconsOutputDirectory = `${moduleDirectory}/icons`;
const umbracoSvgDirectory = `${moduleDirectory}/svgs`;
const iconMapJson = `${moduleDirectory}/icon-dictionary.json`;

const lucideSvgDirectory = 'node_modules/lucide-static/icons';

const run = async () => {
	var icons = await collectDictionaryIcons();
	icons = await collectDiskIcons(icons);
	writeIconsToDisk(icons);
	generateJSON(icons);
};

const collectDictionaryIcons = async () => {

	const rawData = readFileSync(iconMapJson);
	const fileRaw = rawData.toString();
	const fileJSON = JSON.parse(fileRaw);

	let icons = [];

	// Lucide:
	fileJSON.lucide.forEach((iconDef) => {
		// enables leaving things uncommented via underscore
		if(iconDef.file && iconDef.name) {
			const path = lucideSvgDirectory + "/" + iconDef.file;

			try {
				const rawData = readFileSync(path);
				// For Lucide icons specially we adjust the stroke width to 1.75 as that looks better for our usage in backoffice:
				const svg = rawData.toString().replace('stroke-width="2"', 'stroke-width="1.75"');
				const iconFileName = iconDef.name;

				const icon = {
					name: iconDef.name,
					legacy: iconDef.legacy,
					fileName: iconFileName,
					svg,
					output: `${iconsOutputDirectory}/${iconFileName}.js`,
				};

				icons.push(icon);
			} catch(e) {
				console.log(`Could not load file: '${path}'`);
			}
		}
	});

	return icons;
};

const collectDiskIcons = async (icons) => {
	const iconPaths = await glob(`${umbracoSvgDirectory}/icon-*.svg`);

	iconPaths.forEach((path) => {
		const rawData = readFileSync(path);
		const svg = rawData.toString();
		const pattern = /\/([^/]+)\.svg$/;

		const match = path.match(pattern);

		if (!match) {
			console.log('No match found.');
			return;
		}

		const SVGFileName = match[1];
		const iconFileName = SVGFileName.replace('.svg', '');
		const iconName = iconFileName;

		// Only append not already defined icons:
		if(!icons.find(x => x.name === iconName)) {

			const icon = {
				name: iconName,
				legacy: true,
				fileName: iconFileName,
				svg,
				output: `${iconsOutputDirectory}/${iconFileName}.js`,
			};

			icons.push(icon);
		}
	});

	return icons;
};

const writeIconsToDisk = (icons) => {
	icons.forEach((icon) => {
		const content = 'export default `' + icon.svg + '`;';

		writeFileWithDir(icon.output, content, (err) => {
			if (err) {
				// eslint-disable-next-line no-undef
				console.log(err);
			}

			// eslint-disable-next-line no-undef
			console.log(`icon: ${icon.name} generated`);
		});
	});
};

const generateJSON = (icons) => {
	const JSONPath = `${iconsOutputDirectory}/icons.json`;

	const iconDescriptors = icons.map((icon) => {
		return {
			name: icon.name,
			legacy: icon.legacy,
			path: `./icons/${icon.fileName}.js`,
		};
	});

	const content = `${JSON.stringify(iconDescriptors)}`;

	writeFileWithDir(JSONPath, content, (err) => {
		if (err) {
			// eslint-disable-next-line no-undef
			console.log(err);
		}

		// eslint-disable-next-line no-undef
		console.log('icon manifests generated');
	});
};

const writeFileWithDir = (path, contents, cb) => {
	mkdir(getDirName(path), { recursive: true }, function (err) {
		if (err) return cb(err);

		writeFile(path, contents, cb);
	});
};

run();
