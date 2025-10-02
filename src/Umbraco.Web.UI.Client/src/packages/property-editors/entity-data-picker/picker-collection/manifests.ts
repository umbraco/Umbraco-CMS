import {
	UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS,
	UMB_ENTITY_DATA_PICKER_COLLECTION_REPOSITORY_ALIAS,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ENTITY_DATA_PICKER_COLLECTION_REPOSITORY_ALIAS,
		name: 'Entity Data Picker Collection Repository',
		api: () => import('./entity-data-picker-item.repository.js'),
	},
	{
		type: 'collectionMenu',
		kind: 'default',
		alias: UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS,
		name: 'Entity Data Picker Collection Menu',
		meta: {
			collectionRepositoryAlias: UMB_ENTITY_DATA_PICKER_COLLECTION_REPOSITORY_ALIAS,
		},
	},
];
