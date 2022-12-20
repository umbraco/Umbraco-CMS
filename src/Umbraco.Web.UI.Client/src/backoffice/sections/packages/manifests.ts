import type { ManifestSection, ManifestSectionView } from '@umbraco-cms/models';

const sectionAlias = 'Umb.Section.Packages';

const section: ManifestSection = {
	type: 'section',
	alias: sectionAlias,
	name: 'Packages Section',
	weight: 200,
	meta: {
		label: 'Packages',
		pathname: 'packages',
	},
};

const sectionsViews: Array<ManifestSectionView> = [
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Packages.Repo',
		name: 'Packages Repo Section View',
		loader: () => import('./views/repo/section-view-packages-repo.element'),
		meta: {
			sections: [sectionAlias],
			label: 'Packages',
			pathname: 'packages',
			weight: 300,
			icon: 'umb:cloud',
		},
	},
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Packages.Installed',
		name: 'Installed Packages Section View',
		loader: () => import('./views/installed/section-view-packages-installed.element'),
		meta: {
			sections: [sectionAlias],
			label: 'Installed',
			pathname: 'installed',
			weight: 200,
			icon: 'umb:box',
		},
	},
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Packages.Builder',
		name: 'Packages Builder Section View',
		loader: () => import('./views/created/section-view-packages-created.element'),
		meta: {
			sections: [sectionAlias],
			label: 'Created',
			pathname: 'created',
			weight: 100,
			icon: 'umb:files',
		},
	},
];

export const manifests = [section, ...sectionsViews];
