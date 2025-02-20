import { UMB_DOCUMENT_PUBLISHING_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_PUBLISHING_REPOSITORY_ALIAS,
		name: 'Document Publishing Repository',
		api: () => import('./document-publishing.repository.js'),
	},
];
