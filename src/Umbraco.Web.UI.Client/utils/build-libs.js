import * as fs from 'fs';
import { exec } from 'child_process';

const libsDistFolder = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice/libs';
const typesDistFolder = '../Umbraco.Web.UI.New/dts';
const libs = fs.readdirSync('./libs');

for (let i = 0; i < libs.length; i++) {
	const lib = libs[i];
	const libFolder = './libs/' + lib;
	if (fs.statSync(libFolder).isDirectory()) {
		const libPackage = libFolder + '/rollup.config.js';
		if (!fs.existsSync(libPackage)) {
			continue;
		}

		console.log('Installing ' + lib + '...');
		exec('npx rollup -c rollup.config.js', { cwd: libFolder }, function (error) {
			if (error) {
				console.error('Error installing ' + lib + '!');
				console.error(error);
			} else {
				console.log('Installed ' + lib + '.');

				copyDistFromLib(lib, `${libFolder}/dist`);
				findAndCopyTypesForLib(lib);
			}
		});
	}
}

function copyDistFromLib(libName, distPath) {
	console.log(`Copying ${libName} to StaticAssets`);

	const destPath = `${libsDistFolder}/${libName}`;

	try {
		fs.readdirSync(distPath).forEach(file => fs.cpSync(`${distPath}/${file}`, `${destPath}/${file}`, { recursive: true }));
		console.log(`Copied ${libName}`);
	} catch (err) {
		console.error(`Error copying ${libName}`);
		console.error(err);
	}
}

/**
 * This function copies the content of the index.d.ts file from the lib into
 * the ${typesDistFolder}/global.d.ts file and wrap it with
 * a declare module statement using the lib name.
 */
function findAndCopyTypesForLib(libName) {
	console.log(`Copying ${libName} types to ${typesDistFolder}`);

	const sourceFile = `${libsDistFolder}/${libName}/index.d.ts`;
	const destPath = `${typesDistFolder}/${libName}/index.d.ts`;

	try {
		fs.cpSync(sourceFile, destPath, { recursive: true });
		const content = fs.readFileSync(destPath, 'utf-8');
		fs.writeFileSync(destPath, wrapLibTypeContent(libName, content));
		console.log(`Copied ${libName} types`);
	} catch (err) {
		console.error(`Error copying ${libName} types`);
		console.error(err);
	}
}

function wrapLibTypeContent(libName, content) {
	return `declare module "@umbraco-cms/${libName}" {
	${content.replace(/declare/g, '')}
}
`;
}
