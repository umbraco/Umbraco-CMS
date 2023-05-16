import { UMB_USER_SECTION_ALIAS } from '../../user-section/manifests';
import type { ManifestSectionView } from '@umbraco-cms/backoffice/extension-registry';

const sectionsViews: Array<ManifestSectionView> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.UserGroups',
		name: 'User Groups Section View',
		loader: () => import('./section-view-user-groups.element'),
		weight: 100,
		meta: {
			label: 'User Groups',
			pathname: 'user-groups',
			icon: 'umb:users',
		},
		conditions: {
			sections: [UMB_USER_SECTION_ALIAS],
		},
	},
];

export const manifests = [...sectionsViews];
