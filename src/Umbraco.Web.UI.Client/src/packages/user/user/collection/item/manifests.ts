import { UMB_USER_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityCollectionItemCard',
		alias: 'Umb.EntityCollectionItemCard.User',
		name: 'User Entity Collection Item Card',
		element: () => import('./user-collection-item-card.element.js'),
		forEntityTypes: [UMB_USER_ENTITY_TYPE],
	},
];
