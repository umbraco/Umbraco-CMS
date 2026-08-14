import {
	UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
	UMB_ELEMENT_FOLDER_RECYCLE_BIN_REPOSITORY_ALIAS,
} from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_RECYCLE_BIN_REPOSITORY_ALIAS,
		name: 'Element Recycle Bin Repository',
		api: () => import('./element-recycle-bin.repository.js'),
	},
	{
		type: 'repository',
		alias: UMB_ELEMENT_FOLDER_RECYCLE_BIN_REPOSITORY_ALIAS,
		name: 'Element Folder Recycle Bin Repository',
		api: () => import('./element-folder-recycle-bin.repository.js'),
	},
];
