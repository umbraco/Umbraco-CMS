import {
	UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_REPOSITORY_ALIAS,
	UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_STORE_ALIAS,
} from './constants.js';
import { UmbPropertyEditorDataSourceItemStore } from './item.store.js';
import { UmbPropertyEditorDataSourceItemRepository } from './item.repository.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_REPOSITORY_ALIAS,
		name: 'Property Editor Data Source Item Repository',
		api: UmbPropertyEditorDataSourceItemRepository,
	},
	{
		type: 'itemStore',
		alias: UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_STORE_ALIAS,
		name: 'Property Editor Data Source Item Store',
		api: UmbPropertyEditorDataSourceItemStore,
	},
];
