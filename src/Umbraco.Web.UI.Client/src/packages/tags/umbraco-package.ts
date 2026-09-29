import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...repositoryManifests, ...propertyEditorManifests];

export const name = 'Umbraco.Core.TagManagement';
export const version = '0.0.1';
export const extensions = [
	{
		name: 'Tags Management Bundle',
		alias: 'Umb.Bundle.TagsManagement',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
