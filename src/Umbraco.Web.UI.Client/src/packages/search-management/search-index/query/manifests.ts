import { UMB_SEARCH_QUERY_REPOSITORY_ALIAS } from '../constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		name: 'Umbraco Search Query Repository',
		alias: UMB_SEARCH_QUERY_REPOSITORY_ALIAS,
		api: () => import('./search-query.repository.js'),
	},
];
