import type { ManifestSectionSidebarApp } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestSectionSidebarApp> = [
	{
		type: 'sectionSidebarApp',
		alias: 'Umb.SectionSidebarItem.LanguageSelect',
		name: 'App Language Select Section Sidebar Item',
		js: () => import('./app-language-select.element.js'),
		weight: 900,
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Content',
			},
		],
	},
];

export const manifests = [...entityActions];
