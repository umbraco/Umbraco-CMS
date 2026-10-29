import { manifests as segmentManifests } from './manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...segmentManifests];

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
