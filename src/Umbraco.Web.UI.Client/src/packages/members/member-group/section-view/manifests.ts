import type { ManifestSectionView } from '@umbraco-cms/backoffice/extension-registry';

const sectionsViews: Array<ManifestSectionView> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.MemberGroup',
		name: 'Member Group Section View',
		js: () => import('./member-group-section-view.element.js'),
		weight: 100,
		meta: {
			label: 'Member Groups',
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

export const manifests = [...sectionsViews];
