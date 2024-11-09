import { UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS, UMB_STYLESHEET_FOLDER_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS,
		name: 'Stylesheet Folder Repository',
		api: () => import('./stylesheet-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_STYLESHEET_FOLDER_STORE_ALIAS,
		name: 'Stylesheet Folder Store',
		api: () => import('./stylesheet-folder.store.js'),
	},
];
