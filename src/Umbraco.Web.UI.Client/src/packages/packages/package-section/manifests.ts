import { UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS } from '@umbraco-cms/backoffice/section';

const sectionAlias = 'Umb.Section.Packages';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'section',
		alias: sectionAlias,
		name: 'Packages Section',
		weight: 700,
		meta: {
			label: '#sections_packages',
			pathname: 'packages',
		},
		conditions: [
			{
				alias: UMB_SECTION_USER_PERMISSION_CONDITION_ALIAS,
				match: sectionAlias,
			},
		],
	},
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Packages.Marketplace',
		name: 'Packages Marketplace Section View',
		element: () => import('./views/marketplace/packages-marketplace-section-view.element.js'),
		weight: 300,
		meta: {
			label: 'Packages',
			pathname: 'packages',
			icon: 'icon-cloud',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: sectionAlias,
			},
		],
	},
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Packages.Installed',
		name: 'Installed Packages Section View',
		element: () => import('./views/installed/installed-packages-section-view.element.js'),
		weight: 200,
		meta: {
			label: '#packager_installed',
			pathname: 'installed',
			icon: 'icon-box',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: sectionAlias,
			},
		],
	},
	{
		type: 'sectionView',
		alias: 'Umb.SectionView.Packages.Builder',
		name: 'Packages Builder Section View',
		element: () => import('./views/created/created-packages-section-view.element.js'),
		weight: 100,
		meta: {
			label: '#packager_created',
			pathname: 'created',
			icon: 'icon-files',
		},
		conditions: [
			{
				alias: 'Umb.Condition.SectionAlias',
				match: sectionAlias,
			},
		],
	},
];
