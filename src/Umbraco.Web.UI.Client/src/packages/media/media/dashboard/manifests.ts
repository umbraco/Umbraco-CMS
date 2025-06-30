import { UMB_SECTION_ALIAS_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		alias: 'Umb.Dashboard.Media',
		name: 'Media Dashboard',
		element: () => import('./media-dashboard.element.js'),
		weight: 200,
		meta: {
			label: '#general_media',
			pathname: 'media',
		},
		conditions: [
			{
				alias: UMB_SECTION_ALIAS_CONDITION_ALIAS,
				match: 'Umb.Section.Media',
			},
		],
	},
];
