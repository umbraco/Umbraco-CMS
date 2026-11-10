import { UMB_MOVE_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_DOCUMENT_TYPE_FOLDER_REPOSITORY_ALIAS,
		name: 'Move Document Type Folder Repository',
		api: () => import('./document-type-folder-move.repository.js'),
	},
];
