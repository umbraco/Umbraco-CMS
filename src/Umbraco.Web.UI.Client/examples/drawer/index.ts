import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';
import type { ManifestDrawerApp } from '@umbraco-cms/backoffice/drawer';

const exampleDrawer: ManifestDrawerApp = {
	type: 'drawerApp',
	alias: 'example.drawer.poc',
	name: 'Example Drawer (POC)',
	element: () => import('./example-drawer.element.js'),
};

const exampleDrawerDashboard: ManifestDashboard = {
	type: 'dashboard',
	alias: 'example.dashboard.drawer.poc',
	name: 'Example Drawer Dashboard',
	element: () => import('./example-drawer-dashboard.element.js'),
	weight: 900,
	meta: {
		label: 'Drawer POC',
		pathname: 'drawer-poc',
	},
	conditions: [
		{
			alias: 'Umb.Condition.SectionAlias',
			match: 'Umb.Section.Content',
		},
	],
};

export default [exampleDrawer, exampleDrawerDashboard];
