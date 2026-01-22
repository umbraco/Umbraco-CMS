import { UMB_DOCUMENT_TYPE_TEMPLATE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_TYPE_TEMPLATE_REPOSITORY_ALIAS,
		name: 'Document Type Template Repository',
		api: () => import('./document-type-template.repository.js'),
	},
];
