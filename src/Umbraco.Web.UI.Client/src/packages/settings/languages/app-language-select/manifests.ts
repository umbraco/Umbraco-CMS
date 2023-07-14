import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const entityActions: Array<ManifestTypes> = [
	{
		type: 'globalContext',
		alias: 'Umb.GlobalContext.AppLanguage',
		name: 'App Language Context',
		loader: () => import('./app-language.context.js'),
	},
	{
		type: 'sectionSidebarApp',
		alias: 'Umb.SectionSidebarItem.LanguageSelect',
		name: 'App Language Select Section Sidebar Item',
		loader: () => import('./app-language-select.element.js'),
		weight: 900,
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				value: 'Umb.Section.Content',
			},
		],
	},
];

export const manifests = [...entityActions];
