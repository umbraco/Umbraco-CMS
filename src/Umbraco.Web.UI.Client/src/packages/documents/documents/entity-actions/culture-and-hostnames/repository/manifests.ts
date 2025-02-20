import { UMB_DOCUMENT_CULTURE_AND_HOSTNAMES_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_CULTURE_AND_HOSTNAMES_REPOSITORY_ALIAS,
		name: 'Document Culture And Hostnames Repository',
		api: () => import('./culture-and-hostnames.repository.js'),
	},
];
