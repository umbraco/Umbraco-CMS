import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as dashboardManifests } from './dashboard/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...dashboardManifests,
	...entityActionManifests,
	...menuItemManifests,
	...repositoryManifests,
	...searchManifests,
	...treeManifests,
	...workspaceManifests,
	{
		name: 'Dictionary Backoffice Entry Point',
		alias: 'Umb.EntryPoint.Dictionary',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
