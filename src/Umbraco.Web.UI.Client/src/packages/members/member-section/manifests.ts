import type { ManifestSection, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const section: ManifestSection = {
	type: 'section',
	alias: 'Umb.Section.Members',
	name: 'Members Section',
	weight: 500,
	meta: {
		label: '#sections_member',
		pathname: 'member-management',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionUserPermission',
			match: 'Umb.Section.Members',
		},
	],
};

export const manifests: Array<ManifestTypes> = [section];
