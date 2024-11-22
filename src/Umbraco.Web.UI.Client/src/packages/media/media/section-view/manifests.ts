export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Media',
		name: 'Media Section View',
		element: () => import('./media-section-view.element.js'),
		weight: 200,
		meta: {
			label: '#general_media',
			pathname: 'media',
			icon: 'icon-user',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Media',
			},
		],
	},
];
