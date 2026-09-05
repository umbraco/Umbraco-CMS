import { UMB_ELEMENT_FOLDER_ITEM_REPOSITORY_ALIAS, UMB_ELEMENT_FOLDER_ITEM_STORE_ALIAS } from './constants.js';
import { UmbElementFolderItemStore } from './element-folder-item.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_FOLDER_ITEM_REPOSITORY_ALIAS,
		name: 'Element Folder Item Repository',
		api: () => import('./element-folder-item.repository.js'),
	},
	{
		type: 'itemStore',
		alias: UMB_ELEMENT_FOLDER_ITEM_STORE_ALIAS,
		name: 'Element Folder Item Store',
		api: UmbElementFolderItemStore,
	},
];
