import { UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS, UMB_ELEMENT_FOLDER_STORE_ALIAS } from './constants.js';
import { manifests as itemManifests } from './item/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		name: 'Element Folder Repository',
		api: () => import('./element-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_ELEMENT_FOLDER_STORE_ALIAS,
		name: 'Element Folder Store',
		api: () => import('./element-folder.store.js'),
	},
	...itemManifests,
];
