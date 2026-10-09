import { manifests as collectionManifests } from './collection/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...collectionManifests];

export const name = 'Umbraco.Core.Segment';
export const extensions = [
	{
		name: 'Segment Bundle',
		alias: 'Umb.Bundle.Segment',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
