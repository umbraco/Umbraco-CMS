import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		name: 'Media Type Search Provider',
		alias: 'Umb.SearchProvider.MediaType',
		type: 'searchProvider',
		api: () => import('./media-type.search-provider.js'),
		weight: 500,
		meta: {
			label: 'Media Types',
		},
	},
	{
		name: 'Media Type Search Result Item ',
		alias: 'Umb.SearchResultItem.MediaType',
		type: 'searchResultItem',
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
	},
];
