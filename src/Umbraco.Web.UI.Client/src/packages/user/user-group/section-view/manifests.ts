import { UMB_USER_MANAGEMENT_SECTION_ALIAS } from '../../user-section/manifests.js';
import type { ManifestSectionView } from '@umbraco-cms/backoffice/extension-registry';

const sectionsViews: Array<ManifestSectionView> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.UserGroup',
		name: 'User Group Section View',
		js: () => import('./user-group-section-view.element.js'),
		weight: 100,
		meta: {
			label: 'User Groups',
			pathname: 'user-groups',
			icon: 'icon-users',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: UMB_USER_MANAGEMENT_SECTION_ALIAS,
			},
		],
	},
];

export const manifests = [...sectionsViews];
