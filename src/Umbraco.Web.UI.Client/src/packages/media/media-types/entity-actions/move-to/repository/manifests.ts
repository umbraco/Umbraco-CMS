import { UMB_MOVE_MEDIA_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_MEDIA_TYPE_REPOSITORY_ALIAS,
		name: 'Move Media Type Repository',
		api: () => import('./media-type-move.repository.js'),
	},
];
