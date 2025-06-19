import { UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DICTIONARY_EXPORT_REPOSITORY_ALIAS,
		name: 'Dictionary Export Repository',
		api: () => import('./dictionary-export.repository.js'),
	},
];
