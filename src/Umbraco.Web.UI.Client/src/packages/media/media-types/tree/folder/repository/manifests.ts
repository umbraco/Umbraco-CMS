import { UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS, UMB_MEDIA_TYPE_FOLDER_STORE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Media Type Folder Repository',
		api: () => import('./media-type-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_MEDIA_TYPE_FOLDER_STORE_ALIAS,
		name: 'Media Type Folder Store',
		api: () => import('./media-type-folder.store.js'),
	},
];
