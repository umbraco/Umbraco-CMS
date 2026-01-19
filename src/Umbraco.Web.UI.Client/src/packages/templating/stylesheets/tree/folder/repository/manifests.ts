import { UMB_STYLESHEET_FOLDER_REPOSITORY_ALIAS, UMB_STYLESHEET_FOLDER_STORE_ALIAS } from './constants.js';
import { UmbStylesheetFolderStore } from './stylesheet-folder.store.js';

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
		api: UmbStylesheetFolderStore,
	},
];
