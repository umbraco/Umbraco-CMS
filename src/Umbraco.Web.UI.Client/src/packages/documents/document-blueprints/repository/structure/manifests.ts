import { UMB_BLUEPRINT_DOCUMENT_TYPE_STRUCTURE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_BLUEPRINT_DOCUMENT_TYPE_STRUCTURE_REPOSITORY_ALIAS,
		name: 'Blueprint Document Type Structure Repository',
		api: () => import('./blueprint-document-type-structure.repository.js'),
	},
];
