import { UMB_USER_MANAGEMENT_SECTION_ALIAS } from './constants.js';
import { manifests as sectionSidebarAppManifests } from './sidebar-app/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import type {
	ManifestSection,
	ManifestTypes,
	UmbBackofficeManifestKind,
} from '@umbraco-cms/backoffice/extension-registry';

const section: ManifestSection = {
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
			alias: 'Umb.Condition.SectionUserPermission',
			match: UMB_USER_MANAGEMENT_SECTION_ALIAS,
		},
	],
};

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	section,
	...sectionSidebarAppManifests,
	...menuManifests,
];
