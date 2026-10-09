import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import * as entryPointModule from './entry-point.js';

export const manifests: Array<UmbExtensionManifest> = [
	...propertyEditorManifests,
	...treeManifests,
	...repositoryManifests,
	{
		name: 'Static File Backoffice Entry Point',
		alias: 'Umb.EntryPoint.StaticFile',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];

export const name = 'Umbraco.Core.StaticFileManagement';
export const extensions = [
	{
		name: 'Static File Management Bundle',
		alias: 'Umb.Bundle.StaticFileManagement',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
