/**
 * This module is used to detect circular dependencies in the Umbraco backoffice.
 * It is used in the build process to ensure that we don't have circular dependencies.
 * @example node devops/circular/index.js src
 * @author Umbraco HQ
 */

import madge from 'madge';
import { join } from 'path';
import { mkdirSync } from 'fs';

const __dirname = import.meta.dirname;
const IS_GITHUB_ACTIONS = process.env.GITHUB_ACTIONS === 'true';
const IS_AZURE_PIPELINES = process.env.TF_BUILD === 'true';
const baseDir = process.argv[2] || 'src';
const specificPaths = (process.argv[3] || '').split(',');

console.log('Scanning for circular dependencies in:', baseDir);

const madgeSetup = await madge(specificPaths, {
	baseDir,
	fileExtensions: ['ts'],
	tsConfig: join(baseDir, 'tsconfig.build.json'),
	detectiveOptions: {
		ts: {
			skipTypeImports: true,
			skipAsyncImports: true
		}
	}
});

console.log('-'.repeat(80));

const circular = madgeSetup.circular();

if (circular.length) {
	console.error(circular.length, 'circular dependencies detected:\n');
	for (let i = 0; i < circular.length; i++) {
		printCircularDependency(circular[i], i + 1);
	}
	console.error('\nPlease fix the circular dependencies before proceeding.\n');

	try {
		const imagePath = join(__dirname, '../../madge');
		mkdirSync(imagePath, { recursive: true });
		const image = await madgeSetup.image(join(imagePath, 'circular.svg'), true);
		console.log('Circular dependencies graph generated:', image);
	} catch { console.warn('No image generated. Make sure Graphviz is in your $PATH if you want a visualization'); }

	// TODO: Remove this check and set an exit with argument 1 when we have fixed all circular dependencies.
	if (circular.length > 9)
		process.exit(1)
	else
		process.exit(0)
}

console.log('\nNo circular dependencies detected.\n');
process.exit(0);

/**
 *
 * @param {string[]} circular The circular dependencies.
 * @param {number} idx The index of the circular dependency.
 */
function printCircularDependency(circular, idx) {
	circular = circular.map(file => `${baseDir}/${file}`);
	const circularPath = circular.join(' -> ');

	if (IS_GITHUB_ACTIONS) {
		console.error(`::error file=${circular[0]},title=Circular dependency::Circular dependencies detected: ${circularPath}`);
	}
	else if (IS_AZURE_PIPELINES) {
		console.error(`##vso[task.logissue type=error;sourcepath=${circular[0]};]Circular dependencies detected: ${circularPath}`);
	} else {
		console.error(idx, '=', circularPath, '\n');
	}

}
