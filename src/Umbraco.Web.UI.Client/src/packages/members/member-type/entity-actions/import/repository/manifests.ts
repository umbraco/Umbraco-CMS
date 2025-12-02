import { UMB_MEMBER_TYPE_IMPORT_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_TYPE_IMPORT_REPOSITORY_ALIAS,
		name: 'Import Member Type Repository',
		api: () => import('./member-type-import.repository.js'),
	},
];
