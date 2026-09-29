import { manifests as modalManifests } from './modal/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...modalManifests, ...repositoryManifests];

export const name = 'Umbraco.Core.EmbeddedMedia';
export const extensions = [
	{
		name: 'Embedded Media Bundle',
		alias: 'Umb.Bundle.EmbeddedMedia',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
