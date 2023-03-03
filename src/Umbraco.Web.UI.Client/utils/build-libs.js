import * as fs from 'fs';
import { exec } from 'child_process';

const libsDistFolder = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice/libs';
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
			}
		});
	}
}

function copyDistFromLib(libName, distPath) {
	console.log(`Copying ${libName} to StaticAssets`);
	const targetFolder = `${libsDistFolder}/${libName}`;

	fs.cp(distPath, targetFolder, { recursive: true }, function (err) {
		if (err) {
			console.error(`Error copying ${libName}`);
			console.error(err);
		} else {
			console.log(`Copied ${libName}`);
			findAndCopyTypesForLib(libName);
		}
	});
}

/**
 * Look in the ./types/libs folder for a folder with the same name as the {libName} parameter
 * and copy those types into the `${libsDistFolder}/${libName}` folder.
 * Wrap the types from the index.d.ts file as a new module called "@umbraco-cms/{libName}".
 */
function findAndCopyTypesForLib(libName) {
	console.log('Installing types for', libName);
	const typesFolder = './types/libs';
	const libTypesFolder = `${typesFolder}/${libName}`;
	if (fs.existsSync(libTypesFolder)) {
		const libTypesTargetFolder = `${libsDistFolder}/${libName}`;
		fs.cpSync(libTypesFolder, `${libTypesTargetFolder}/types`, { recursive: true });
		fs.writeFileSync(`${libTypesTargetFolder}/index.d.ts`, wrapLibTypeContent(libName), {});
	}
}

function wrapLibTypeContent(libName) {
	return `
	declare module "@umbraco-cms/${libName}";
	`;
}
