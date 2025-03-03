import { UMB_MOVE_DICTIONARY_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_DICTIONARY_REPOSITORY_ALIAS,
		name: 'Move Dictionary Repository',
		api: () => import('./dictionary-move.repository.js'),
	},
];
