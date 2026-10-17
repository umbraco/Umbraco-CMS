export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'headerApp',
		alias: 'Mock.HeaderApp.MockSetSwitcher',
		name: 'Mock Set Switcher Header App',
		element: () => import('./mock-set-header-app.element.js'),
		weight: 1000,
	},
	{
		type: 'headerApp',
		alias: 'Mock.HeaderApp.ExampleSwitcher',
		name: 'Example Switcher Header App',
		element: () => import('./examples-header-app.element.js'),
		weight: 999,
	},
];
