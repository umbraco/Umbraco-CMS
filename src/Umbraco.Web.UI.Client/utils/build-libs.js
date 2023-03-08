import * as fs from 'fs';
import { exec } from 'child_process';

const libsDistFolder = '../Umbraco.Cms.StaticAssets/wwwroot/umbraco/backoffice/libs';
const typesDistFolder = '../Umbraco.Web.UI.New';
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

	const sourceFile = `${distPath}/index.js`;
	const destFile = `${libsDistFolder}/${libName}.js`;

	try {
		fs.copyFileSync(sourceFile, destFile);
		console.log(`Copied ${libName}`);
		findAndCopyTypesForLib(libName, distPath);
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
function findAndCopyTypesForLib(libName, distPath) {
	console.log(`Copying ${libName} types to ${typesDistFolder}`);

	const sourceFile = `${distPath}/index.d.ts`;
	const destFile = `${typesDistFolder}/global.d.ts`;

	try {
		// Take the content of sourceFile and wrap it with a declare module statement in destFile
		const content = fs.readFileSync(sourceFile, 'utf8');
		if (!content) {
			return;
		}
		fs.writeFileSync(destFile, wrapLibTypeContent(libName, content) + "\n", { flag: 'a' });
		console.log(`Copied ${libName} types`);
	} catch (err) {
		console.error(`Error copying ${libName} types`);
		console.error(err);
	}
}

function wrapLibTypeContent(libName, content) {
	return `
	declare module "@umbraco-cms/${libName}" {
		${content}
	};
	`;
}
