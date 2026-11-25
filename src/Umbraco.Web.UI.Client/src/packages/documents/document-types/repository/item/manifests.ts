import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_TYPE_ITEM_STORE_ALIAS } from './constants.js';
import { UmbDocumentTypeItemStore } from './document-type-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.DocumentType',
		name: 'Document Type Entity Item Reference',
		element: () => import('./document-type-item-ref.element.js'),
		forEntityTypes: [UMB_DOCUMENT_TYPE_ENTITY_TYPE],
	},
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_ITEM_REPOSITORY_ALIAS,
		name: 'Document Type Item Repository',
		api: () => import('./document-type-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DOCUMENT_TYPE_ITEM_STORE_ALIAS,
		name: 'Document Type Item Store',
		api: UmbDocumentTypeItemStore,
	},
];
