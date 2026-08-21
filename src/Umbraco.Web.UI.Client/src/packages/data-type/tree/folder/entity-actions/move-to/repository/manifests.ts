import { UMB_MOVE_DATA_TYPE_FOLDER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_DATA_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Move Data Type Folder Repository',
		api: () => import('./data-type-folder-move.repository.js'),
	},
];
