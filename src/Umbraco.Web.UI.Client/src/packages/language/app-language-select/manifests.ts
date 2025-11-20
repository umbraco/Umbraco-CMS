import { UMB_CONTENT_SECTION_ALIAS } from '@umbraco-cms/backoffice/content';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';
export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'sectionSidebarApp',
		alias: 'Umb.SectionSidebarItem.LanguageSelect',
		name: 'App Language Select Section Sidebar Item',
		js: () => import('./app-language-select.element.js'),
		weight: 900,
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_CONTENT_SECTION_ALIAS,
			},
			{
				alias: 'Umb.Condition.MultipleAppLanguages',
			},
		],
	},
];
