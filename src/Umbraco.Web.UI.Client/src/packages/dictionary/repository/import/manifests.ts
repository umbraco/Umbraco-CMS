import { UMB_DICTIONARY_IMPORT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DICTIONARY_IMPORT_REPOSITORY_ALIAS,
		name: 'Dictionary Import Repository',
		api: () => import('./dictionary-import.repository.js'),
	},
];
