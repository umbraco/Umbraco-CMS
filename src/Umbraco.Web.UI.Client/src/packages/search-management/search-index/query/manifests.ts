import { UMB_SEARCH_QUERY_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_SEARCH_LEGACY_QUERY_REPOSITORY_ALIAS, loadWithDeprecationWarning } from '../legacy-aliases.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		name: 'Umbraco Search Query Repository',
		alias: UMB_SEARCH_QUERY_REPOSITORY_ALIAS,
		api: () => import('./search-query.repository.js'),
	},
	{
		type: 'repository',
		name: 'Umbraco Search Query Repository (deprecated alias)',
		alias: UMB_SEARCH_LEGACY_QUERY_REPOSITORY_ALIAS,
		api: () =>
			loadWithDeprecationWarning(
				UMB_SEARCH_LEGACY_QUERY_REPOSITORY_ALIAS,
				UMB_SEARCH_QUERY_REPOSITORY_ALIAS,
				() => import('./search-query.repository.js'),
			),
	},
];
