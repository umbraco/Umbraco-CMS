import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest> = [
	...entityActionManifests,
	...menuManifests,
	...repositoryManifests,
	...treeManifests,
	...workspaceManifests,
	{
		name: 'Document Blueprint Backoffice Entry Point',
		alias: 'Umb.BackofficeEntryPoint.DocumentBlueprint',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
