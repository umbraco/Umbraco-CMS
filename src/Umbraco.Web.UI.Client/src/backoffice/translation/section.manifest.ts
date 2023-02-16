import type { ManifestDashboard, ManifestSection } from '@umbraco-cms/models';

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

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.TranslationDictionary',
		name: 'Dictionary Translation Dashboard',
		elementName: 'umb-dashboard-translation-dictionary',
		loader: () => import('./dashboards/dictionary/dashboard-translation-dictionary.element'),
		meta: {
			label: 'Dictionary overview',
			sections: [sectionAlias],
			pathname: '',
		},
	},
];


export const manifests = [section, ...dashboards];
