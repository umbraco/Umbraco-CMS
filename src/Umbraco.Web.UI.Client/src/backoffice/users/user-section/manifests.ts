import type { ManifestSection, ManifestSectionView } from '@umbraco-cms/backoffice/extensions-registry';

const sectionAlias = 'Umb.Section.Users';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Users Section',
	weight: 100,
	meta: {
		label: 'Users',
		pathname: 'users',
	},
};

const sectionsViews: Array<ManifestSectionView> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Users.Users',
		name: 'Users Section View',
		loader: () => import('./views/users/section-view-users.element'),
		weight: 200,
		meta: {
			label: 'Users',
			pathname: 'users',
			icon: 'umb:user',
		},
		conditions: {
			sections: [sectionAlias],
		},
	},
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Users.UserGroups',
		name: 'User Groups Section View',
		loader: () => import('./views/user-groups/section-view-user-groups.element'),
		weight: 100,
		meta: {
			label: 'User Groups',
			pathname: 'user-groups',
			icon: 'umb:users',
		},
		conditions: {
			sections: [sectionAlias],
		},
	},
];

export const manifests = [section, ...sectionsViews];
