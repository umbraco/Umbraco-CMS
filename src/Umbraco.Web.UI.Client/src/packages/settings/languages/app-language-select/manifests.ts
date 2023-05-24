import { ManifestSectionSidebarApp } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestSectionSidebarApp> = [
	{
		type: 'sectionSidebarApp',
		alias: 'Umb.SectionSidebarItem.LanguageSelect',
		name: 'App Language Select Section Sidebar Item',
		loader: () => import('./app-language-select.element.js'),
		weight: 900,
		conditions: {
			sections: ['Umb.Section.Content'],
		},
	},
];

export const manifests = [...entityActions];
