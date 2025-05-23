import { UMB_MOVE_DOCUMENT_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_DOCUMENT_TYPE_REPOSITORY_ALIAS,
		name: 'Move Document Type Repository',
		api: () => import('./document-type-move.repository.js'),
	},
];
