import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';

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
			match: 'Umb.Section.Content',
		},
	],
};
