import type { ManifestSection, ManifestSectionView } from '@umbraco-cms/models';

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
		meta: {
			sections: [sectionAlias],
			label: 'Users',
			pathname: 'users',
			weight: 200,
			icon: 'umb:user',
		},
	},
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Users.UserGroups',
		name: 'User Groups Section View',
		loader: () => import('./views/user-groups/section-view-user-groups.element'),
		meta: {
			sections: [sectionAlias],
			label: 'User Groups',
			pathname: 'user-groups',
			weight: 100,
			icon: 'umb:users',
		},
	},
];

export const manifests = [section, ...sectionsViews];
