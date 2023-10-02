import type { ManifestSection } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_SECTION_ALIAS = 'Umb.Section.Users';

const section: ManifestSection = {
	type: 'section',
	alias: UMB_USER_SECTION_ALIAS,
	name: 'Users Section',
	weight: 100,
	meta: {
		label: 'Users',
		pathname: 'users',
	},
};

export const manifests = [section];
