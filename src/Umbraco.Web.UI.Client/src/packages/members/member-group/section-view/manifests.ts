export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.MemberGroup',
		name: 'Member Group Section View',
		js: () => import('./member-group-section-view.element.js'),
		weight: 100,
		meta: {
			label: '#treeHeaders_memberGroups',
			pathname: 'member-groups',
			icon: 'icon-users',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: 'Umb.Section.Members',
			},
		],
	},
];
