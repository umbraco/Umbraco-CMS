export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		kind: 'default',
		name: 'Example Dashboard With Collection',
		alias: 'Example.Dashboard.WithCollection',
		element: () => import('./dashboard-with-collection.element.js'),
		weight: 3000,
		meta: {
			label: 'Collection Example',
			pathname: 'collection-example',
		},
	},
];
