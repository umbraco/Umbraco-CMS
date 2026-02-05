import { readFileSync, writeFileSync, existsSync } from 'fs';
import { join } from 'path';
import glob from 'tiny-glob'

console.log('[Prepublish] Cleansing package.json');

const packageFile = './package.json';
const packageJson = JSON.parse(readFileSync(packageFile, 'utf8'));

// Remove all DevDependencies
delete packageJson.devDependencies;

// Convert version to a looser range that allows plugin developers to use newer versions
// while still enforcing a minimum version and safety ceiling
const looseVersionRange = (version) => {
	// Parse semantic version
	const match = version.match(/^(0|[1-9]\d*)\.(\d+)\.(\d+)(-.*)?(\+.*)?$/);
	if (!match) {
		console.warn('Could not parse version:', version, 'keeping original');
		return version;
	}

	const major = match[1];
	const minor = match[2];
	const patch = match[3];

	// For pre-release (0.x.y), use floor at current version and ceiling at 1.0.0
	if (major === '0') {
		return `>=${major}.${minor}.${patch} <1.0.0`;
	}

	// For stable versions (1.x.y+), use major.x.x to allow any patch/minor within that major
	return `${major}.x.x`;
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
const workspacePromises = workspaces.map(async workspaceGlob => {
	// Use glob to find the workspace path
	const localWorkspace = workspaceGlob.replace(/\.\/src/, './dist-cms');
	const workspacePaths = await glob(localWorkspace, { cwd: './', absolute: true });

	workspacePaths.forEach(workspace => {
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
				console.log('Hoisting dependency:', key, 'from workspace:', workspace, 'with version:', value, 'loosened to:', loosenedVersion);
				packageJson.peerDependencies[key] = loosenedVersion;
			});
		}
	})
});

// Wait for all workspace processing to complete
await Promise.all(workspacePromises);

// Remove the workspaces field from the root package.json
delete packageJson.workspaces;

// Write the package.json back to disk
writeFileSync(packageFile, JSON.stringify(packageJson, null, 2), 'utf8');
