import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityCollectionItemCard',
		alias: 'Umb.EntityCollectionItemCard.Document',
		name: 'Document Entity Collection Item Card',
		element: () => import('./document-collection-item-card.element.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	},
];
