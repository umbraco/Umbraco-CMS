import { UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_SEARCH_LEGACY_COLLECTION_REPOSITORY_ALIAS, loadWithDeprecationWarning } from '../../legacy-aliases.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		name: 'Umbraco Search Collection Repository',
		alias: UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
		api: () => import('./search-collection.repository.js'),
	},
	{
		type: 'repository',
		name: 'Umbraco Search Collection Repository (deprecated alias)',
		alias: UMB_SEARCH_LEGACY_COLLECTION_REPOSITORY_ALIAS,
		api: () =>
			loadWithDeprecationWarning(
				UMB_SEARCH_LEGACY_COLLECTION_REPOSITORY_ALIAS,
				UMB_SEARCH_COLLECTION_REPOSITORY_ALIAS,
				() => import('./search-collection.repository.js'),
			),
	},
];
