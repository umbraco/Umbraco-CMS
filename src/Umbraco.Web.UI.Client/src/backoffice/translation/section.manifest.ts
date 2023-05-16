import type { ManifestDashboard, ManifestSection, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

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

const menuSectionSidebarApp: ManifestTypes = {
	type: 'sectionSidebarApp',
	kind: 'menu',
	alias: 'Umb.SidebarMenu.Dictionary',
	name: 'Dictionary Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Dictionary',
		menu: 'Umb.Menu.Dictionary',
	},
	conditions: {
		sections: [sectionAlias],
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
			pathname: '',
		},
		conditions: {
			sections: [sectionAlias],
		},
	},
];

export const manifests = [section, menuSectionSidebarApp, ...dashboards];
