import { UMB_DICTIONARY_ROOT_ENTITY_TYPE } from './dictionary/entity.js';
import type { ManifestDashboard, ManifestSection, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const sectionAlias = 'Umb.Section.Dictionary';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Dictionary Section',
	weight: 100,
	meta: {
		label: 'Dictionary',
		pathname: 'dictionary',
	},
};

const menuSectionSidebarApp: ManifestTypes = {
	type: 'sectionSidebarApp',
	kind: 'menuWithEntityActions',
	alias: 'Umb.SidebarMenu.Dictionary',
	name: 'Dictionary Sidebar Menu',
	weight: 100,
	meta: {
		label: 'Dictionary',
		menu: 'Umb.Menu.Dictionary',
		entityType: UMB_DICTIONARY_ROOT_ENTITY_TYPE,
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: sectionAlias,
		},
	],
};

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Dictionary.Overview',
		name: 'Dictionary Overview Dashboard',
		element: () => import('./dictionary/dashboards/dictionary-overview-dashboard.element.js'),
		meta: {
			label: 'Dictionary overview',
			pathname: '',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: sectionAlias,
			},
		],
	},
];

export const manifests = [section, menuSectionSidebarApp, ...dashboards];
