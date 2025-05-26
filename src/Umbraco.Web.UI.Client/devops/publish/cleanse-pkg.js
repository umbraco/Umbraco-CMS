import { readFileSync, writeFileSync } from 'fs';

console.log('[Prepublish] Cleansing package.json');

const packageFile = './package.json';
const packageJson = JSON.parse(readFileSync(packageFile, 'utf8'));

// Remove all DevDependencies
delete packageJson.devDependencies;

// Rename dependencies to peerDependencies
packageJson.peerDependencies = { ...packageJson.dependencies };
delete packageJson.dependencies;

// Update workspaces path
packageJson.workspaces = packageJson.workspaces.map((workspace) => {
	// Rename the 'src/' prefix to 'dist-cms/' from each workspace path
	return workspace.replace('./src', './dist-cms');
});

// Write the package.json back to disk
writeFileSync(packageFile, JSON.stringify(packageJson, null, 2), 'utf8');
