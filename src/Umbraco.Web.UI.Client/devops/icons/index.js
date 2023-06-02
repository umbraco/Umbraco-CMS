import { readFileSync, writeFile, mkdir } from 'fs';
import * as globModule from 'tiny-glob';
import * as pathModule from 'path';

const path = pathModule.default;
const getDirName = path.dirname;
const glob = globModule.default;

const moduleDirectory = 'src/shared/icon-registry';
const iconsSVGDirectory = `${moduleDirectory}/svgs`;
const iconsOutputDirectory = `${moduleDirectory}/icons`;

const run = async () => {
	const icons = await collectIcons();
	outputIcons(icons);
	generateJSON(icons);
};

const collectIcons = async () => {
	const iconPaths = await glob(`${iconsSVGDirectory}/icon-*.svg`);

	let icons = [];

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
		const iconName = iconFileName.replace('icon-', '').replace('.svg', '');

		const icon = {
			src: path,
			SVGFileName,
			iconFileName,
			name: iconName,
			svg,
			output: `${iconsOutputDirectory}/${iconFileName}.js`,
		};

		icons.push(icon);
	});

	return icons;
};

const outputIcons = (icons) => {
	icons.forEach((icon) => {
		const content = 'export default `' + icon.svg + '`;';

		writeFileWithDir(`${icon.output}`, content, (err) => {
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
		console.log(icon);
		return {
			name: `umb:${icon.name}`,
			path: `./icons/${icon.iconFileName}.js`,
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
