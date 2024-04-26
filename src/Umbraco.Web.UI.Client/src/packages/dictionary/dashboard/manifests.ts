import { UMB_DICTIONARY_SECTION_ALIAS } from '../section/index.js';
import type { ManifestDashboard, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const dashboards: Array<ManifestDashboard> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Dictionary.Overview',
		name: 'Dictionary Overview Dashboard',
		element: () => import('./dictionary-overview-dashboard.element.js'),
		meta: {
			label: '#dictionaryItem_overviewTitle',
			pathname: '',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_DICTIONARY_SECTION_ALIAS,
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [...dashboards];
