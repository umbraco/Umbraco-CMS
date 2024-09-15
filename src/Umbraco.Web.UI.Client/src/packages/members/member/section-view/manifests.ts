export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Member',
		name: 'Member Section View',
		js: () => import('./member-section-view.element.js'),
		weight: 200,
		meta: {
			label: '#treeHeaders_member',
			pathname: 'members',
			icon: 'icon-user',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Members',
			},
		],
	},
];
