import { UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS, UMB_SCRIPT_FOLDER_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_SCRIPT_FOLDER_REPOSITORY_ALIAS,
		name: 'Script Folder Repository',
		api: () => import('./script-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_SCRIPT_FOLDER_STORE_ALIAS,
		name: 'Script Folder Store',
		api: () => import('./script-folder.store.js'),
	},
];
