import { ManifestSectionSidebarItem } from '@umbraco-cms/extensions-registry';

const entityActions: Array<ManifestSectionSidebarItem> = [
	{
		type: 'sectionSidebarItem',
		alias: 'Umb.SectionSidebarItem.LanguageSelect',
		name: 'App Language Select Section Sidebar Item',
		loader: () => import('./app-language-select.element'),
		meta: {
			sections: ['Umb.Section.Content'],
		},
	},
];

export const manifests = [...entityActions];
