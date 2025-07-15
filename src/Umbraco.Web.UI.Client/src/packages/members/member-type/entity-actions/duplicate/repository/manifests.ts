import { UMB_DUPLICATE_MEMBER_TYPE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_DUPLICATE_MEMBER_TYPE_REPOSITORY_ALIAS,
		name: 'Duplicate Member Type Repository',
		api: () => import('./member-type-duplicate.repository.js'),
	},
];
