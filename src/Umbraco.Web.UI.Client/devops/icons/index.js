import { readFileSync, writeFile, mkdir, rmSync } from 'fs';
import * as globModule from 'tiny-glob';
import * as pathModule from 'path';

const path = pathModule.default;
const getDirName = path.dirname;
const glob = globModule.default;

const moduleDirectory = 'src/packages/core/icon-registry';
const iconsOutputDirectory = `${moduleDirectory}/icons`;
const umbracoSvgDirectory = `${moduleDirectory}/svgs`;
const iconMapJson = `${moduleDirectory}/icon-dictionary.json`;

const lucideSvgDirectory = 'node_modules/lucide-static/icons';
const simpleIconsSvgDirectory = 'node_modules/simple-icons/icons';

const run = async () => {
	// Empty output directory:
	rmSync(iconsOutputDirectory, { recursive: true });

	var icons = await collectDictionaryIcons();
	icons = await collectDiskIcons(icons);
	writeIconsToDisk(icons);
	generateJS(icons);
};

const collectDictionaryIcons = async () => {
	const rawData = readFileSync(iconMapJson);
	const fileRaw = rawData.toString();
	const fileJSON = JSON.parse(fileRaw);

	let icons = [];

	// Lucide:
	fileJSON.lucide.forEach((iconDef) => {
		if (iconDef.file && iconDef.name) {
			const path = lucideSvgDirectory + '/' + iconDef.file;

			try {
				const rawData = readFileSync(path);
				// For Lucide icons specially we adjust the icons a bit for them to work in our case: [NL]
				let svg = rawData.toString().replace('  width="24"\n', '');
				svg = svg.replace('  height="24"\n', '');
				svg = svg.replace('stroke-width="2"', 'stroke-width="1.75"');
				const iconFileName = iconDef.name;

				const icon = {
					name: iconDef.name,
					legacy: iconDef.legacy, // TODO: Deprecated, remove in v.17.
					hidden: iconDef.legacy ?? iconDef.internal,
					fileName: iconFileName,
					svg,
					output: `${iconsOutputDirectory}/${iconFileName}.ts`,
				};

				icons.push(icon);
			} catch (e) {
				console.log(`[Lucide] Could not load file: '${path}'`);
			}
		}
	});

	// SimpleIcons:
	fileJSON.simpleIcons.forEach((iconDef) => {
		if (iconDef.file && iconDef.name) {
			const path = simpleIconsSvgDirectory + '/' + iconDef.file;

			try {
				const rawData = readFileSync(path);
				let svg = rawData.toString();
				const iconFileName = iconDef.name;

				// SimpleIcons need to use fill="currentColor"
				const pattern = /fill=/g;
				if (!pattern.test(svg)) {
					svg = svg.replace(/<path/g, '<path fill="currentColor"');
				}

				const icon = {
					name: iconDef.name,
					legacy: iconDef.legacy,
					fileName: iconFileName,
					svg,
					output: `${iconsOutputDirectory}/${iconFileName}.ts`,
				};

				icons.push(icon);
			} catch (e) {
				console.log(`[SimpleIcons] Could not load file: '${path}'`);
			}
		}
	});

	// Umbraco:
	fileJSON.umbraco.forEach((iconDef) => {
		if (iconDef.file && iconDef.name) {
			const path = umbracoSvgDirectory + '/' + iconDef.file;

			try {
				const rawData = readFileSync(path);
				const svg = rawData.toString();
				const iconFileName = iconDef.name;

				const icon = {
					name: iconDef.name,
					legacy: iconDef.legacy,
					fileName: iconFileName,
					svg,
					output: `${iconsOutputDirectory}/${iconFileName}.ts`,
				};

				icons.push(icon);
			} catch (e) {
				console.log(`[Umbraco] Could not load file: '${path}'`);
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
		const parsed = pathModule.parse(path);

		if (!parsed) {
			console.log('No match found for: ', path);
			return;
		}

		const SVGFileName = parsed.name;
		const iconFileName = SVGFileName.replace('.svg', '');
		const iconName = iconFileName;

		// Only append not already defined icons:
		if (!icons.find((x) => x.name === iconName)) {
			// remove legacy for v.17 (Deprecated)
			const icon = {
				name: iconName,
				legacy: true,
				hidden: true,
				fileName: iconFileName,
				svg,
				output: `${iconsOutputDirectory}/${iconFileName}.ts`,
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
			//console.log(`icon: ${icon.name} generated`);
		});
	});
};

const generateJS = (icons) => {
	const JSPath = `${moduleDirectory}/icons.ts`;

	const iconDescriptors = icons.map((icon) => {
		// remove legacy for v.17 (Deprecated)
		return `{
			name: "${icon.name}",
			${icon.legacy ? 'legacy: true,' : ''}
			${icon.hidden ? 'hidden: true,' : ''}
			path: () => import("./icons/${icon.fileName}.js"),
		}`.replace(/\t/g, '').replace(/^\s*[\r\n]/gm, ''); // Regex removes white space [NL] // + regex that removes empty lines. [NL]
	});

	const content = `export default [${iconDescriptors.join(',')}];`;

	writeFileWithDir(JSPath, content, (err) => {
		if (err) {
			// eslint-disable-next-line no-undef
			console.log(err);
		}

		// eslint-disable-next-line no-undef
		console.log('Icons outputted and Icon Manifests generated!');
	});
};

const writeFileWithDir = (path, contents, cb) => {
	mkdir(getDirName(path), { recursive: true }, function (err) {
		if (err) return cb(err);

		writeFile(path, contents, cb);
	});
};

run();
