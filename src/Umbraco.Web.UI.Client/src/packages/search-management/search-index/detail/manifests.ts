import { UMB_SEARCH_DETAIL_REPOSITORY_ALIAS, UMB_SEARCH_DETAIL_STORE_ALIAS } from '../constants.js';
import { UMB_SEARCH_LEGACY_DETAIL_REPOSITORY_ALIAS, loadWithDeprecationWarning } from '../legacy-aliases.js';
import { UmbSearchDetailStore } from './search-detail.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		name: 'Umbraco Search Detail Repository',
		alias: UMB_SEARCH_DETAIL_REPOSITORY_ALIAS,
		api: () => import('./search-detail.repository.js'),
	},
	{
		type: 'repository',
		name: 'Umbraco Search Detail Repository (deprecated alias)',
		alias: UMB_SEARCH_LEGACY_DETAIL_REPOSITORY_ALIAS,
		api: () =>
			loadWithDeprecationWarning(
				UMB_SEARCH_LEGACY_DETAIL_REPOSITORY_ALIAS,
				UMB_SEARCH_DETAIL_REPOSITORY_ALIAS,
				() => import('./search-detail.repository.js'),
			),
	},
	{
		// No compatibility registration for the former 'UmbSearchStore' alias: a second registration
		// instantiates a second store, and both would provide themselves at the same context token.
		type: 'store',
		name: 'Umbraco Search Detail Store',
		alias: UMB_SEARCH_DETAIL_STORE_ALIAS,
		api: UmbSearchDetailStore,
	},
];
