import { UMB_USER_MANAGEMENT_SECTION_ALIAS } from './constants.js';
import { manifests as sectionSidebarAppManifests } from './sidebar-app/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'section',
		alias: UMB_USER_MANAGEMENT_SECTION_ALIAS,
		name: 'User Management Section',
		weight: 600,
		meta: {
			label: '#sections_users',
			pathname: 'user-management',
		},
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: UMB_USER_MANAGEMENT_SECTION_ALIAS,
			},
		],
	},
	...sectionSidebarAppManifests,
	...menuManifests,
];
