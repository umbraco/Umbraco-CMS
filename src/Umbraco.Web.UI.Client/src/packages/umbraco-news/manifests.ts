import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';
import { UMB_CONTENT_SECTION_ALIAS } from '@umbraco-cms/backoffice/content';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const dashboard: ManifestDashboard = {
	type: 'dashboard',
	alias: 'Umb.Dashboard.UmbracoNews',
	name: 'Umbraco News Dashboard',
	element: () => import('./umbraco-news-dashboard.element.js'),
	weight: 20,
	meta: {
		label: '#dashboardTabs_contentIntro',
	},
	conditions: [
		{
			alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
			match: UMB_CONTENT_SECTION_ALIAS,
		},
	],
};
