import { manifests as searchIndexManifests } from './search-index/manifests.js';
import { manifests as examineManifests } from './examine/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...searchIndexManifests, ...examineManifests];

export const name = 'Umbraco.Core.SearchManagement';
export const extensions = [
	{
		name: 'Search Management Bundle',
		alias: 'Umb.Bundle.SearchManagement',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
