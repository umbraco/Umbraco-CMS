import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestDashboard } from '@umbraco-cms/backoffice/extensions-registry';

const dashboard: ManifestDashboard = {
	type: 'dashboard',
	alias: 'Umb.Dashboard.UmbracoNews',
	name: 'Umbraco News Dashboard',
	loader: () => import('./umbraco-news-dashboard.element'),
	weight: 20,
	meta: {
		label: 'Welcome',
		pathname: 'welcome',
	},
	conditions: {
		sections: ['Umb.Section.Content'],
	},
};

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => extensionRegistry.register(dashboard);
