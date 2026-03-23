import {
	UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
	UMB_ENTITY_DATA_PICKER_COLLECTION_MENU_ALIAS,
	UMB_ENTITY_DATA_PICKER_COLLECTION_REPOSITORY_ALIAS,
	UMB_ENTITY_DATA_PICKER_COLLECTION_TEXT_FILTER_ALIAS,
} from './constants.js';
import { UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS } from '../conditions/constants.js';
import { UMB_COLLECTION_ALIAS_CONDITION } from '@umbraco-cms/backoffice/collection';

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
	{
		type: 'collectionTextFilter',
		kind: 'default',
		alias: UMB_ENTITY_DATA_PICKER_COLLECTION_TEXT_FILTER_ALIAS,
		name: 'Entity Data Picker Collection Text Filter',
		conditions: [
			{
				alias: UMB_COLLECTION_ALIAS_CONDITION,
				match: UMB_ENTITY_DATA_PICKER_COLLECTION_ALIAS,
			},
			{
				alias: UMB_ENTITY_DATA_PICKER_SUPPORTS_TEXT_FILTER_CONDITION_ALIAS,
			},
		],
	},
	...collectionViewManifests,
];
