import { manifests as conditionsManifests } from './conditions/manifests.js';
import { manifests as entityActionsManifests } from './entity-actions/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest> = [
	...conditionsManifests,
	...entityActionsManifests,
	...menuManifests,
	...modalManifests,
	...repositoryManifests,
	...searchManifests,
	...treeManifests,
	...workspaceManifests,
	{
		name: 'Template Backoffice Entry Point',
		alias: 'Umb.EntryPoint.Template',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
