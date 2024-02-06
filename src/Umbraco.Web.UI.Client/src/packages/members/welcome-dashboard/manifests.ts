import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Member.Welcome',
		name: 'Member Welcome Dashboard',
		weight: 10,
		js: () => import('./member-welcome-dashboard.element.js'),
		meta: {
			label: 'Members',
			pathname: 'members',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Members',
			},
		],
	},
];
