export const manifests = [
	{
		type: 'section',
		alias: 'MyBundle.Section.Custom',
		name: 'Custom Section',
		element: () => import('./section.js'),
		weight: 1,
		meta: {
			label: 'My Bundle Section',
			pathname: 'my-custom-bundle',
		},
	},
];
