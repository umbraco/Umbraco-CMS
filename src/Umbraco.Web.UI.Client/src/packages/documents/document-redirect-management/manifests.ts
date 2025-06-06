import { UMB_CONTENT_SECTION_ALIAS } from '@umbraco-cms/backoffice/content';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.RedirectManagement',
		name: 'Redirect Management Dashboard',
		element: () => import('./dashboard-redirect-management.element.js'),
		weight: 10,
		meta: {
			label: '#dashboardTabs_contentRedirectManager',
			pathname: 'redirect-management',
		},
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: UMB_CONTENT_SECTION_ALIAS,
			},
		],
	},
];
