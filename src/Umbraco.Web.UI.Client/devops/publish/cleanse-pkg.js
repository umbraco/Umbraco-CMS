/* eslint-disable local-rules/enforce-umbraco-external-imports */
import { readFileSync, writeFileSync, existsSync } from 'fs';
import { join } from 'path';
import glob from 'tiny-glob';
import semver from 'semver';

console.log('[Prepublish] Cleansing package.json');

const packageFile = './package.json';
const packageJson = JSON.parse(readFileSync(packageFile, 'utf8'));

// Remove all DevDependencies
delete packageJson.devDependencies;

// Convert version to a looser range that allows plugin developers to use newer versions
// while still enforcing a minimum version and safety ceiling
const looseVersionRange = (version) => {
	// If it already has a caret and major >= 1, keep it as-is (e.g., ^3.3.1)
	if (version.startsWith('^')) {
		const minVersion = semver.minVersion(version);
		if (minVersion && minVersion.major >= 1) {
			console.log('Keeping caret range for stable version:', version);
			return version;
		}
	}

	// Extract minimum version from a range (e.g., ^0.85.0 -> 0.85.0)
	const minVersion = semver.minVersion(version);
	if (!minVersion) {
		console.warn('Could not parse version:', version, 'keeping original');
		return version;
	}

	const major = minVersion.major;
	const minor = minVersion.minor;
	const patch = minVersion.patch;

	// For pre-release (0.x.y), use floor at current version and ceiling at 1.0.0
	if (major === 0) {
		return `>=${major}.${minor}.${patch} <1.0.0`;
	}

	// For stable versions without caret (exact versions), use >=X.Y.Z <(MAJOR+1).0.0
	const nextMajor = major + 1;
	return `>=${major}.${minor}.${patch} <${nextMajor}.0.0`;
};

// Rename dependencies to peerDependencies with looser version ranges
packageJson.peerDependencies = {};
Object.entries(packageJson.dependencies || {}).forEach(([key, value]) => {
	packageJson.peerDependencies[key] = looseVersionRange(value);
	console.log('Converting to peer dependency:', key, 'from', value, 'to', packageJson.peerDependencies[key]);
});
delete packageJson.dependencies;

// Iterate all workspaces and hoist the dependencies to the root package.json
const workspaces = packageJson.workspaces || [];
const workspacePromises = workspaces.map(async (workspaceGlob) => {
	// Use glob to find the workspace path
	const localWorkspace = workspaceGlob.replace(/\.\/src/, './dist-cms');
	const workspacePaths = await glob(localWorkspace, { cwd: './', absolute: true });

	workspacePaths.forEach((workspace) => {
		const workspacePackageFile = join(workspace, 'package.json');

		// Ensure the workspace package.json exists
		if (!existsSync(workspacePackageFile)) {
			// If the package.json does not exist, log a warning and continue
			console.warn(`No package.json found in workspace: ${workspace}`);
			return;
		}

		const workspacePackageJson = JSON.parse(readFileSync(workspacePackageFile, 'utf8'));

		// Move dependencies from the workspace to the root package.json
		if (workspacePackageJson.dependencies) {
			Object.entries(workspacePackageJson.dependencies).forEach(([key, value]) => {
				const loosenedVersion = looseVersionRange(value);
				console.log(
					'Hoisting dependency:',
					key,
					'from workspace:',
					workspace,
					'with version:',
					value,
					'loosened to:',
					loosenedVersion,
				);
				packageJson.peerDependencies[key] = loosenedVersion;
			});
		}
	});
});

// Wait for all workspace processing to complete
await Promise.all(workspacePromises);

// Remove the workspaces field from the root package.json
delete packageJson.workspaces;

// Write the package.json back to disk
writeFileSync(packageFile, JSON.stringify(packageJson, null, 2), 'utf8');
