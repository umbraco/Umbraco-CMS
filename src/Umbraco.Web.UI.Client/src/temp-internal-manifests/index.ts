import type { ManifestTypes, ManifestWithLoader } from '@umbraco-cms/models';

export const internalManifests: Array<ManifestWithLoader<ManifestTypes>> = [
	{
		type: 'editorView',
		alias: 'Umb.Editor.Packages.Overview',
		name: 'Packages Editor Overview View',
		elementName: 'umb-packages-overview',
		loader: () => import('../backoffice/sections/packages/packages-overview.element'),
		weight: 10,
		meta: {
			icon: 'document',
			label: 'Packages',
			pathname: 'repo',
			editors: ['Umb.Editor.Packages'],
		},
	},
	{
		type: 'editorView',
		alias: 'Umb.Editor.Packages.Installed',
		name: 'Packages Editor Installed View',
		elementName: 'umb-packages-installed',
		loader: () => import('../backoffice/sections/packages/packages-installed.element'),
		weight: 0,
		meta: {
			icon: 'document',
			label: 'Installed',
			pathname: 'installed',
			editors: ['Umb.Editor.Packages'],
		},
	},
];
