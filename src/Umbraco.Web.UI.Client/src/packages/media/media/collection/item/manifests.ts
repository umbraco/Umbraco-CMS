import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityCollectionItemCard',
		alias: 'Umb.EntityCollectionItemCard.Media',
		name: 'Media Entity Collection Item Card',
		element: () => import('./media-collection-item-card.element.js'),
		forEntityTypes: [UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE],
	},
];
