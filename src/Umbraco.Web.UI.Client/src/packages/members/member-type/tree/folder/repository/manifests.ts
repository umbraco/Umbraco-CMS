import { UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS, UMB_MEMBER_TYPE_FOLDER_STORE_ALIAS } from './constants.js';
import { UmbMemberTypeFolderStore } from './member-type-folder.store.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Member Type Folder Repository',
		api: () => import('./member-type-folder.repository.js'),
	},
	{
		type: 'store',
		alias: UMB_MEMBER_TYPE_FOLDER_STORE_ALIAS,
		name: 'Member Type Folder Store',
		api: UmbMemberTypeFolderStore,
	},
];
