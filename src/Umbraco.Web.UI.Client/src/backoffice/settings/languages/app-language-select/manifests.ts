import { ManifestSectionSidebarApp } from '@umbraco-cms/extensions-registry';

const entityActions: Array<ManifestSectionSidebarApp> = [
	{
		type: 'sectionSidebarApp',
		alias: 'Umb.SectionSidebarItem.LanguageSelect',
		name: 'App Language Select Section Sidebar Item',
		loader: () => import('./app-language-select.element'),
		meta: {
			sections: ['Umb.Section.Content'],
		},
	},
];

export const manifests = [...entityActions];
