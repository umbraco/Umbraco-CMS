import type { ManifestSection } from '@umbraco-cms/models';

const section: ManifestSection = {
	type: 'section',
	alias: 'Umb.Section.Members',
	name: 'Members Section',
	weight: 400,
	meta: {
		label: 'Members',
		pathname: 'members',
	},
};

export const manifests = [section];
