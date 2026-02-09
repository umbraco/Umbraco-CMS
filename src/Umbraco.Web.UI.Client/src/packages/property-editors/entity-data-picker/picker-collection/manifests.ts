import {
	UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
	UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS,
	UMB_ENTITY_DATA_PICKER_COLLECTION_REPOSITORY_ALIAS,
} from './constants.js';

import { manifests as collectionViewManifests } from './views/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ENTITY_DATA_PICKER_COLLECTION_REPOSITORY_ALIAS,
		name: 'Entity Data Picker Collection Repository',
		api: () => import('./entity-data-picker-collection.repository.js'),
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
	{
		type: 'collection',
		kind: 'default',
		alias: UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
		name: 'Entity Data Picker Collection',
		meta: {
			repositoryAlias: UMB_ENTITY_DATA_PICKER_COLLECTION_REPOSITORY_ALIAS,
		},
	},
	...collectionViewManifests,
];
