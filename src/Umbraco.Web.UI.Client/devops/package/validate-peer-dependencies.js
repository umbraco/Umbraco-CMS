/* eslint-disable local-rules/enforce-umbraco-external-imports */
import { existsSync, readFileSync, writeFileSync } from 'fs';
import { relative, resolve } from 'path';
import { listPeerDependencies, toPeerDependencies } from './peer-dependencies.js';
import semver from 'semver';

const clientProjectRoot = resolve(import.meta.dirname, '../../');
const repositoryRoot = resolve(clientProjectRoot, '../../');
const templateFolder = resolve(repositoryRoot, 'templates/UmbracoExtension/Client');
const templateFolderName = relative(repositoryRoot, templateFolder);
const baselineFile = resolve(import.meta.dirname, 'peer-dependencies.baseline.json');
const baselineFileName = relative(clientProjectRoot, baselineFile);

const readJson = (file) => JSON.parse(readFileSync(file, 'utf8'));
// A fixed locale keeps the baseline's order the same on every machine
const byName = (a, b) => a.localeCompare(b, 'en');

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

// An extension that depends on the previous floor with a caret range must still satisfy the published peer dependencies
if (process.argv.includes('--update-baseline')) {
	const sortedNames = Object.keys(peerDependencies).sort(byName);
	const snapshot = Object.fromEntries(sortedNames.map((name) => [name, peerDependencies[name]]));
	writeFileSync(baselineFile, `${JSON.stringify(snapshot, null, '\t')}\n`, 'utf8');
	console.log(`Updated ${baselineFileName}`);
}

const baseline = existsSync(baselineFile) ? readJson(baselineFile) : {};
const breakingChanges = [];
const otherChanges = [];
[...new Set([...Object.keys(baseline), ...Object.keys(peerDependencies)])].sort(byName).forEach((name) => {
	const before = baseline[name];
	const after = peerDependencies[name];
	if (before === after) return;

	if (!before) {
		breakingChanges.push(`    added:   ${name} ${after}`);
	} else if (!after) {
		otherChanges.push(`    removed: ${name} ${before}`);
	} else if (!semver.intersects(`^${semver.minVersion(before).version}`, after)) {
		breakingChanges.push(`    changed: ${name} ${before} -> ${after}`);
	} else {
		otherChanges.push(`    changed: ${name} ${before} -> ${after}`);
	}
});

if (breakingChanges.length > 0) {
	errors.push(
		[
			`Breaking changes to the published peer dependencies, compared with ${baselineFileName}:`,
			...breakingChanges,
			`  A changed range no longer accepts what a caret range on the previous floor allows, so an extension that depends`,
			`  on that floor fails "npm install" with ERESOLVE. An added peer dependency is a new requirement on every extension.`,
			`  Either is a breaking change in a patch or minor release. If the change is intended, run`,
			`  "npm run package:update-peer-baseline" and commit the updated baseline.`,
		].join('\n'),
	);
}

if (otherChanges.length > 0) {
	console.log(
		[
			'--- Peer dependency notice (not a failure) ---',
			'',
			`The published peer dependencies changed without breaking extensions, compared with ${baselineFileName}:`,
			...otherChanges,
			`  Run "npm run package:update-peer-baseline" to refresh the baseline.`,
			'',
		].join('\n'),
	);
}

if (errors.length > 0) {
	console.error(`--- Peer dependency validation failed ---\n\n${errors.join('\n\n')}\n`);
	process.exit(1);
}

console.log('--- Peer dependencies validated successfully. ---');
