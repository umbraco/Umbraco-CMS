import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';
import { UMB_CONTENT_SECTION_ALIAS } from '@umbraco-cms/backoffice/content';

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
			alias: 'Umb.Condition.SectionAlias',
			match: UMB_CONTENT_SECTION_ALIAS,
		},
	],
};
