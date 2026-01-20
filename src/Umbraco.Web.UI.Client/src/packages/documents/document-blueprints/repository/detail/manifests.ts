import {
	UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_ALIAS,
} from './constants.js';
import { UmbDocumentBlueprintDetailStore } from './document-blueprint-detail.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_DETAIL_REPOSITORY_ALIAS,
		name: 'Document Blueprint Detail Repository',
		api: () => import('./document-blueprint-detail.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DOCUMENT_BLUEPRINT_DETAIL_STORE_ALIAS,
		name: 'Document Blueprint Detail Store',
		api: UmbDocumentBlueprintDetailStore,
	},
];
