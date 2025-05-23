export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'headerApp',
		alias: 'Umb.HeaderApp.Help',
		name: 'Help Header App',
		element: () => import('./help-header-app.element.js'),
		weight: 500,
	},
];
