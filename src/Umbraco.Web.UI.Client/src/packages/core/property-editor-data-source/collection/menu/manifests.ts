import { UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_REPOSITORY_ALIAS } from '../data/constants.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_MENU_ALIAS } from './constants.js';
import type { ManifestCollectionMenu } from '@umbraco-cms/backoffice/collection';

export const manifests: Array<ManifestCollectionMenu> = [
	{
		type: 'collectionMenu',
		kind: 'default',
		name: 'Property Editor Data Source Collection Menu',
		alias: UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_MENU_ALIAS,
		meta: {
			collectionRepositoryAlias: UMB_PROPERTY_EDITOR_DATA_SOURCE_COLLECTION_REPOSITORY_ALIAS,
		},
	},
];
