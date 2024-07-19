import { UMB_USER_MANAGEMENT_SECTION_ALIAS } from './constants.js';
import type { ManifestSection, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

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

export const manifests: Array<ManifestTypes> = [section];
