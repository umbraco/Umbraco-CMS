import { readFileSync, writeFileSync, existsSync } from 'fs';
import { join } from 'path';
import glob from 'tiny-glob'

console.log('[Prepublish] Cleansing package.json');

const packageFile = './package.json';
const packageJson = JSON.parse(readFileSync(packageFile, 'utf8'));

// Remove all DevDependencies
delete packageJson.devDependencies;

// Rename dependencies to peerDependencies
packageJson.peerDependencies = { ...packageJson.dependencies };
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
				console.log('Hoisting dependency:', key, 'from workspace:', workspace, 'with version:', value);
				packageJson.peerDependencies[key] = value;
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
