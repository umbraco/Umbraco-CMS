import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UMB_MEDIA_SEARCH_PROVIDER_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		name: 'Media Search Provider',
		alias: UMB_MEDIA_SEARCH_PROVIDER_ALIAS,
		type: 'searchProvider',
		api: () => import('./media.search-provider.js'),
		weight: 700,
		meta: {
			label: 'Media',
		},
	},
	{
		name: 'Media Search Result Item',
		alias: 'Umb.SearchResultItem.Media',
		type: 'searchResultItem',
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE],
	},
];
