import { UMB_MOVE_DOCUMENT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_DOCUMENT_REPOSITORY_ALIAS,
		name: 'Move Document Repository',
		api: () => import('./document-move.repository.js'),
	},
];
