import { UMB_DOCUMENT_TYPE_SEARCH_PROVIDER_ALIAS } from '../constants.js';
import { UMB_DOCUMENT_TYPE_GLOBAL_SEARCH_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Document Type Global Search',
		alias: UMB_DOCUMENT_TYPE_GLOBAL_SEARCH_ALIAS,
		type: 'globalSearch',
		weight: 600,
		meta: {
			label: 'Document Types',
			searchProviderAlias: UMB_DOCUMENT_TYPE_SEARCH_PROVIDER_ALIAS,
		},
	},
];
