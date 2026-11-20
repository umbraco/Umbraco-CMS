import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_ITEM_STORE_ALIAS } from './constants.js';
import { UmbMediaTypeItemStore } from './media-type-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.MediaType',
		name: 'Media Type Entity Item Reference',
		element: () => import('./media-type-item-ref.element.js'),
		forEntityTypes: [UMB_MEDIA_TYPE_ENTITY_TYPE],
	},
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_ITEM_REPOSITORY_ALIAS,
		name: 'Media Type Item Repository',
		api: () => import('./media-type-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_MEDIA_TYPE_ITEM_STORE_ALIAS,
		name: 'Media Type Item Store',
		api: UmbMediaTypeItemStore,
	},
];
