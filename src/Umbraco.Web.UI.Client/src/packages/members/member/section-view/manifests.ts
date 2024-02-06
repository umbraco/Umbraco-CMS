import type { ManifestSectionView } from '@umbraco-cms/backoffice/extension-registry';

const sectionsViews: Array<ManifestSectionView> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Member',
		name: 'Member Section View',
		js: () => import('./member-section-view.element.js'),
		weight: 100,
		meta: {
			label: 'Members',
			pathname: 'members',
			icon: 'icon-members',
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
