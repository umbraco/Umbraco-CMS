import { readFileSync, writeFile, mkdir } from 'fs';
import * as globModule from 'tiny-glob';
import * as pathModule from 'path';

const path = pathModule.default;
const getDirName = path.dirname;
const glob = globModule.default;

const moduleDirectory = 'src/core/icon-registry/';
const iconsSVGDirectory = `${moduleDirectory}svgs/`;
const iconsOutputDirectory = `public-assets/icons/`;

const run = async () => {
	const icons = await collectIcons();
	outputIcons(icons);
	generateJSON(icons);
};

const collectIcons = async () => {
	const iconPaths = await glob(`${iconsSVGDirectory}icon-*.svg`);

	let icons = [];

	iconPaths.forEach((path) => {
		const rawData = readFileSync(path);
		const svg = rawData.toString();
		const SVGFileName = path.substring(iconsSVGDirectory.length);
		const iconFileName = SVGFileName.replace('.svg', '');
		const iconName = iconFileName.replace('icon-', '').replace('.svg', '');

		icons.push({
			src: path,
			SVGFileName,
			iconFileName,
			name: iconName,
			svg,
			output: `${iconsOutputDirectory}/${iconFileName}`,
		});
	});

	return icons;
};

const outputIcons = (icons) => {
	icons.forEach((icon) => {
		const content = 'export default `' + icon.svg + '`;';

		writeFileWithDir(`${icon.output}.js`, content, (err) => {
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
	const JSONPath = `${iconsOutputDirectory}icons.json`;

	const iconDescriptors = icons.map((icon) => {
		return {
			name: `umb:${icon.name}`,
			path: `icons/${icon.iconFileName}.js`,
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
