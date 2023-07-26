import { ManifestDashboard } from '@umbraco-cms/backoffice/extension-registry';

export const dashboard: ManifestDashboard = {
	type: 'dashboard',
	alias: 'Umb.Dashboard.UmbracoNews',
	name: 'Umbraco News Dashboard',
	loader: () => import('./umbraco-news-dashboard.element.js'),
	weight: 20,
	meta: {
		label: 'Welcome',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: 'Umb.Section.Content',
		},
	],
};
