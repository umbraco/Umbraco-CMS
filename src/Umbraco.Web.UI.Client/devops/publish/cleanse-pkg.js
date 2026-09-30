/* eslint-disable local-rules/enforce-umbraco-external-imports */
import { readFileSync, writeFileSync } from 'fs';
import { listPeerDependencies, toPeerDependencies } from '../package/peer-dependencies.js';

console.log('[Prepublish] Cleansing package.json');

const packageFile = './package.json';
const packageJson = JSON.parse(readFileSync(packageFile, 'utf8'));

// Remove all DevDependencies
delete packageJson.devDependencies;

// Rename dependencies to peerDependencies with looser version ranges, and hoist the dependencies of all workspaces
// (read from the build output) to the root package.json
const dependencies = await listPeerDependencies(packageJson, { workspaceFolder: './dist-cms' });
dependencies.forEach(({ name, version, peerVersion, workspace }) => {
	if (workspace) {
		console.log(
			'Hoisting dependency:',
			name,
			'from workspace:',
			workspace,
			'with version:',
			version,
			'loosened to:',
			peerVersion,
		);
	} else {
		console.log('Converting to peer dependency:', name, 'from', version, 'to', peerVersion);
	}
});
packageJson.peerDependencies = toPeerDependencies(dependencies);
delete packageJson.dependencies;

// Remove the workspaces field from the root package.json
delete packageJson.workspaces;

// Write the package.json back to disk
writeFileSync(packageFile, JSON.stringify(packageJson, null, 2), 'utf8');
