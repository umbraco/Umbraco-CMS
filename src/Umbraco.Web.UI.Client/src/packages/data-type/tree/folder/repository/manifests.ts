import { UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS, UMB_DATA_TYPE_FOLDER_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Data Type Folder Repository',
		api: () => import('./data-type-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_DATA_TYPE_FOLDER_STORE_ALIAS,
		name: 'Data Type Folder Store',
		api: () => import('./data-type-folder.store.js'),
	},
];
