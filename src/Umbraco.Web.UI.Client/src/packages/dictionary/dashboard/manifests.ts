import { UMB_TRANSLATION_SECTION_ALIAS } from '@umbraco-cms/backoffice/translation';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Dictionary.Overview',
		name: 'Dictionary Overview Dashboard',
		element: () => import('./dictionary-overview-dashboard.element.js'),
		meta: {
			label: '#dictionaryItem_overviewTitle',
			pathname: 'dictionary-overview',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_TRANSLATION_SECTION_ALIAS,
			},
		],
	},
];
