import { UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_DOCUMENT_GLOBAL_SEARCH_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Document Global Search',
		alias: UMB_DOCUMENT_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 800,
		meta: {
			label: 'Documents',
			searchProvider: UMB_DOCUMENT_SEARCH_PROVIDER_ALIAS,
		},
	},
];
