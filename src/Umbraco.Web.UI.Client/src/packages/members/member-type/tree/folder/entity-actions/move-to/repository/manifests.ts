import { UMB_MOVE_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_MEMBER_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Move Member Type Folder Repository',
		api: () => import('./member-type-folder-move.repository.js'),
	},
];
