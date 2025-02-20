import { UMB_MOVE_MEDIA_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_MEDIA_REPOSITORY_ALIAS,
		name: 'Move Media Repository',
		api: () => import('./media-move.repository.js'),
	},
];
