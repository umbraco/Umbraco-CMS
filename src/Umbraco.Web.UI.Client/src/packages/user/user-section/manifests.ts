import type { ManifestSection } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_MANAGEMENT_SECTION_ALIAS = 'Umb.Section.UserManagement';

const section: ManifestSection = {
	type: 'section',
	alias: UMB_USER_MANAGEMENT_SECTION_ALIAS,
	name: 'User Management Section',
	weight: 100,
	meta: {
		label: 'Users',
		pathname: 'user-management',
	},
};

export const manifests = [section];
