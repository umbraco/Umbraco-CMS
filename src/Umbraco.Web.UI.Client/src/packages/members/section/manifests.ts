import { UMB_MEMBER_MANAGEMENT_SECTION_ALIAS } from './constants.js';
import { manifests as sectionSidebarAppManifests } from './sidebar-app/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const section: UmbExtensionManifest = {
	type: 'section',
	alias: UMB_MEMBER_MANAGEMENT_SECTION_ALIAS,
	name: 'Members Section',
	weight: 500,
	meta: {
		label: '#sections_member',
		pathname: 'member-management',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionUserPermission',
			match: UMB_MEMBER_MANAGEMENT_SECTION_ALIAS,
		},
	],
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	section,
	...sectionSidebarAppManifests,
	...menuManifests,
];
