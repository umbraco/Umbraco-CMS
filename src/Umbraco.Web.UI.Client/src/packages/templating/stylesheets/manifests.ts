import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as propertyEditorsManifests } from './property-editors/manifests.js';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest> = [
	...repositoryManifests,
	...menuManifests,
	...treeManifests,
	...workspaceManifests,
	...entityActionManifests,
	...propertyEditorsManifests,
	{
		name: 'Stylesheet Backoffice Entry Point',
		alias: 'Umb.EntryPoint.Stylesheet',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
