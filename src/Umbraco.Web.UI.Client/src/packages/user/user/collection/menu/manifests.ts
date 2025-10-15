import { UMB_USER_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'collectionMenuItem',
		kind: 'default',
		alias: 'Umb.CollectionMenuItem.User',
		name: 'User Collection Menu Item',
		element: () => import('./user-collection-menu-item.element.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
	},
];
