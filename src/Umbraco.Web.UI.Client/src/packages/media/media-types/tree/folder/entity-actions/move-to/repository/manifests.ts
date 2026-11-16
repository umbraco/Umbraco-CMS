import { UMB_MOVE_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_MEDIA_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Move Media Type Folder Repository',
		api: () => import('./media-type-folder-move.repository.js'),
	},
];
