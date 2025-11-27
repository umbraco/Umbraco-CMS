import { UMB_MOVE_MEMBER_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_MEMBER_TYPE_REPOSITORY_ALIAS,
		name: 'Move Member Type Repository',
		api: () => import('./member-type-move.repository.js'),
	},
];
