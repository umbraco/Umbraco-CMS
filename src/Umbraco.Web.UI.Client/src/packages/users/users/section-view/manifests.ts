import { UMB_USER_SECTION_ALIAS } from '../../user-section/manifests.js';
import type { ManifestSectionView } from '@umbraco-cms/backoffice/extension-registry';

const sectionsViews: Array<ManifestSectionView> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Users',
		name: 'Users Section View',
		loader: () => import('./users-section-view.element.js'),
		weight: 200,
		meta: {
			label: 'Users',
			pathname: 'users',
			icon: 'umb:user',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_USER_SECTION_ALIAS,
			},
		],
	},
];

export const manifests = [...sectionsViews];
