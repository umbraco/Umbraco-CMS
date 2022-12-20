import type { ManifestSection } from '@umbraco-cms/models';

const sectionAlias = 'Umb.Section.Translation';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Translation Section',
	weight: 100,
	meta: {
		label: 'Translation',
		pathname: 'translation',
	},
};

export const manifests = [section];
