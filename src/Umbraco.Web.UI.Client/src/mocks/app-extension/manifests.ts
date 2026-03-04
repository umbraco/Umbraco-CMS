export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'headerApp',
		alias: 'Mock.HeaderApp.MockSetSwitcher',
		name: 'Mock Set Switcher Header App',
		element: () => import('./mock-set-header-app.element.js'),
		weight: 1000,
	},
];
