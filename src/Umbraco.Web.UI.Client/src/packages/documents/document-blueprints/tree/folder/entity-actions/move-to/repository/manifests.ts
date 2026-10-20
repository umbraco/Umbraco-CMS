import { UMB_MOVE_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MOVE_DOCUMENT_BLUEPRINT_FOLDER_REPOSITORY_ALIAS,
		name: 'Move Document Blueprint Folder Repository',
		api: () => import('./document-blueprint-folder-move.repository.js'),
	},
];
