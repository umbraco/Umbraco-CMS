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
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Media',
			},
		],
	},
];
