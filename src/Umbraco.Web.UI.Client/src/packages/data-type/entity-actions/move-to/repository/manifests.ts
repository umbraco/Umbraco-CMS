import { UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_DATA_TYPE_REPOSITORY_ALIAS,
		name: 'Move Data Type Repository',
		api: () => import('./data-type-move.repository.js'),
	},
];
