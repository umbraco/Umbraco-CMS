import { UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS, UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS } from './constants.js';
import { UmbDocumentBlueprintItemStore } from './document-blueprint-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS,
		name: 'Document Blueprint Item Repository',
		api: () => import('./document-blueprint-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS,
		name: 'Document Blueprint Item Store',
		api: UmbDocumentBlueprintItemStore,
	},
];
