import { manifests as sectionManifests } from './section/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...sectionManifests, ...menuManifests];

export const name = 'Umbraco.Core.Translation';
export const extensions = [
	{
		name: 'Umbraco Translation Bundle',
		alias: 'Umb.Bundle.Translation',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
