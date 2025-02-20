import { UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS,
		name: 'Document Type Composition Repository',
		api: () => import('./document-type-composition.repository.js'),
	},
];
