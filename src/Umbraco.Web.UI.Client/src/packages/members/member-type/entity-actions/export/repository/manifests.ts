import { UMB_EXPORT_MEMBER_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_EXPORT_MEMBER_TYPE_REPOSITORY_ALIAS,
		name: 'Export Member Type Repository',
		api: () => import('./member-type-export.repository.js'),
	},
];
