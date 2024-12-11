import { UMB_DOCUMENT_CREATE_BLUEPRINT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_CREATE_BLUEPRINT_REPOSITORY_ALIAS,
		name: 'Document Create Blueprint Repository',
		api: () => import('./document-create-blueprint.repository.js'),
	},
];
