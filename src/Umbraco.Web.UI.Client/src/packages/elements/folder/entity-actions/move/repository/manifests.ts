import { UMB_MOVE_ELEMENT_FOLDER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_ELEMENT_FOLDER_REPOSITORY_ALIAS,
		name: 'Move Element Folder Repository',
		api: () => import('./element-folder-move.repository.js'),
	},
];
