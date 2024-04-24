import { UmbDocumentBlueprintItemStore } from './document-blueprint-item.store.js';
import { UmbDocumentBlueprintItemRepository } from './document-blueprint-item.repository.js';
import type { ManifestRepository, ManifestItemStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS = 'Umb.Repository.DocumentBlueprint.Item';
export const UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS = 'Umb.Store.DocumentBlueprint.Item';

const itemRepository: ManifestRepository = {
	type: 'repository',
	alias: UMB_DOCUMENT_BLUEPRINT_ITEM_REPOSITORY_ALIAS,
	name: 'Document Blueprint Item Repository',
	api: UmbDocumentBlueprintItemRepository,
};

const itemStore: ManifestItemStore = {
	type: 'itemStore',
	alias: UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS,
	name: 'Document Blueprint Item Store',
	api: UmbDocumentBlueprintItemStore,
};

export const manifests: Array<ManifestTypes> = [itemRepository, itemStore];
