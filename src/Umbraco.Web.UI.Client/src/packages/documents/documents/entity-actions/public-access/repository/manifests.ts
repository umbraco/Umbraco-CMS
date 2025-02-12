import { UMB_DOCUMENT_PUBLIC_ACCESS_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_PUBLIC_ACCESS_REPOSITORY_ALIAS,
		name: 'Document Public Access Repository',
		api: () => import('./public-access.repository.js'),
	},
];
