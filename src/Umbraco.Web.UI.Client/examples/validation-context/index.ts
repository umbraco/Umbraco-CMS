import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';
import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

const dashboard: ManifestDashboard = {
	type: 'dashboard',
	alias: 'Demo.Dashboard',
	name: 'Demo Dashboard Validation Context',
	weight: 1000,
	element: () => import('./validation-context-dashboard.js'),
	meta: {
		label: 'Validation Context Demo',
		pathname: 'demo',
	},
	conditions: [
		{
			alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
			match: 'Umb.Section.Content',
		},
	],
};

export const manifests = [dashboard];
