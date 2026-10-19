import { UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS } from '../../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		name: 'Umbraco Search Collection Repository',
		alias: UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
		api: () => import('./search-collection.repository.js'),
	},
];
