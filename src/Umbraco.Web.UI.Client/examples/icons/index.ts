export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'icons',
		name: 'Example Dataset Dashboard',
		alias: 'example.dashboard.dataset',
		js: () => import('./icons-dictionary.js'),
	},
	{
		type: 'dashboard',
		name: 'Example Icons Dashboard',
		alias: 'example.dashboard.icons',
		element: () => import('./icons-dashboard.js'),
		weight: 900,
		meta: {
			label: 'Icons example',
			pathname: 'icons-example',
		},
	},
];
