import type { ManifestSection } from '@umbraco-cms/models';

export const manifests: Array<ManifestSection> = [
	{
		type: 'section',
		alias: 'Umb.Section.Content',
		name: 'Content Section',
		elementName: 'umb-content-section',
		loader: () => import('./content/content-section.element'),
		weight: 600,
		meta: {
			label: 'Content',
			pathname: 'content',
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Media',
		name: 'Media Section',
		elementName: 'umb-media-section',
		loader: () => import('./media/media-section.element'),
		weight: 500,
		meta: {
			label: 'Media',
			pathname: 'media',
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Members',
		name: 'Members Section',
		elementName: 'umb-section-members',
		loader: () => import('./members/section-members.element'),
		weight: 400,
		meta: {
			label: 'Members',
			pathname: 'members',
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Settings',
		name: 'Settings Section',
		loader: () => import('./settings/settings-section.element'),
		weight: 300,
		meta: {
			label: 'Settings',
			pathname: 'settings',
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Packages',
		name: 'Packages Section',
		loader: () => import('./packages/section-packages.element'),
		weight: 200,
		meta: {
			label: 'Packages',
			pathname: 'packages',
		},
	},
	{
		type: 'section',
		alias: 'Umb.Section.Users',
		name: 'Users',
		loader: () => import('./users/section-users.element'),
		weight: 100,
		meta: {
			label: 'Users',
			pathname: 'users',
		},
	},
];
