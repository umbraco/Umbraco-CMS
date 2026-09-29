import { manifests as serverEventManifests } from './server-event/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...serverEventManifests];

export const name = 'Umbraco.ManagementApi';
export const extensions = [
	{
		name: 'Management Api Bundle',
		alias: 'Umb.Bundle.ManagementApi',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
