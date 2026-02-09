import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as entityActionsManifests } from './entity-actions/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest> = [
	...repositoryManifests,
	...menuManifests,
	...treeManifests,
	...entityActionsManifests,
	...workspaceManifests,
	{
		name: 'Partial View Backoffice Entry Point',
		alias: 'Umb.EntryPoint.Partial View',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
