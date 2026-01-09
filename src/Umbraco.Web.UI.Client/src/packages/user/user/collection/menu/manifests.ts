import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import { UMB_USER_COLLECTION_REPOSITORY_ALIAS } from '../repository/constants.js';
import { UMB_USER_COLLECTION_MENU_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionMenu',
		kind: 'default',
		alias: UMB_USER_COLLECTION_MENU_ALIAS,
		name: 'User Collection Menu',
		meta: {
			collectionRepositoryAlias: UMB_USER_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'collectionMenuItem',
		kind: 'default',
		alias: 'Umb.CollectionMenuItem.User',
		name: 'User Collection Menu Item',
		element: () => import('./user-collection-menu-item.element.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
	},
];
