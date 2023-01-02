import type { ManifestSection } from '@umbraco-cms/models';

const sectionAlias = 'Umb.Section.Members';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Members Section',
	weight: 400,
	meta: {
		label: 'Members',
		pathname: 'members',
	},
};

export const manifests = [section];
