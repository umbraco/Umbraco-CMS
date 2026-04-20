import { UMB_MEMBER_TYPE_STRUCTURE_REPOSITORY_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_MEMBER_TYPE_STRUCTURE_REPOSITORY_ALIAS,
		name: 'Member Type Structure Repository',
		api: () => import('./member-type-structure.repository.js'),
	},
];
