/* eslint-disable local-rules/enforce-umbraco-external-imports */
import { readFileSync } from 'fs';
import { relative, resolve } from 'path';
import { listPeerDependencies, toPeerDependencies } from './peer-dependencies.js';
import semver from 'semver';

const clientProjectRoot = resolve(import.meta.dirname, '../../');
const repositoryRoot = resolve(clientProjectRoot, '../../');
const templateFolder = resolve(repositoryRoot, 'templates/UmbracoExtension/Client');
const templateFolderName = relative(repositoryRoot, templateFolder);

const readJson = (file) => JSON.parse(readFileSync(file, 'utf8'));

const packageJson = readJson(resolve(clientProjectRoot, 'package.json'));
const dependencies = await listPeerDependencies(packageJson, { cwd: clientProjectRoot });
const peerDependencies = toPeerDependencies(dependencies);
const errors = [];

// A dependency declared with different ranges gets whichever is hoisted last, and the workspaces are hoisted concurrently
Object.keys(peerDependencies).forEach((name) => {
	const declarations = dependencies.filter((dependency) => dependency.name === name);
	if (new Set(declarations.map((dependency) => dependency.peerVersion)).size > 1) {
		const sources = declarations.map(
			({ version, workspace }) => `${workspace ? relative(clientProjectRoot, workspace) : 'package.json'}: ${version}`,
		);
		errors.push(
			`${name} is declared with different ranges (${sources.join(', ')}). Align them, so the published peer range does not depend on the order the workspaces are read in.`,
		);
	}
});

// A freshly scaffolded extension must install without peer dependency conflicts
const templatePackageJson = readJson(resolve(templateFolder, 'package.json'));
['dependencies', 'devDependencies'].forEach((field) => {
	Object.entries(templatePackageJson[field] || {}).forEach(([name, templateRange]) => {
		const peerRange = peerDependencies[name];
		if (!peerRange || !semver.validRange(templateRange)) return;

		if (!semver.subset(templateRange, peerRange)) {
			const templateLabel = `template (${field}):`;
			errors.push(
				[
					`${name}: the Umbraco Extension template is outside the published peer dependency range.`,
					`    ${templateLabel} ${templateRange}`,
					`    ${'peer dependency:'.padEnd(templateLabel.length)} ${peerRange}`,
					`  A new extension fails "npm install" with ERESOLVE. Bump the range in ${templateFolderName}/package.json`,
					`  to one within the peer dependency range. If that changes what the package generates, regenerate what the`,
					`  template commits from it too (e.g. the @hey-api/openapi-ts client in ${templateFolderName}/src/api).`,
				].join('\n'),
			);
		}
	});
});

if (errors.length > 0) {
	console.error(`--- Peer dependency validation failed ---\n\n${errors.join('\n\n')}\n`);
	process.exit(1);
}

console.log('--- Peer dependencies validated successfully. ---');
