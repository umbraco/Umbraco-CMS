import { UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS, UMB_DATA_TYPE_STORE_ALIAS } from './constants.js';
import { UmbDataTypeItemStore } from './data-type-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_ITEM_REPOSITORY_ALIAS,
		name: 'Data Type Item Repository',
		api: () => import('./data-type-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_DATA_TYPE_STORE_ALIAS,
		name: 'Data Type Item Store',
		api: UmbDataTypeItemStore,
	},
];
