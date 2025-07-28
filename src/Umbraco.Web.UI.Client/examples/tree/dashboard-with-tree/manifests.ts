export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		kind: 'default',
		name: 'Example Dashboard With Tree',
		alias: 'Example.Dashboard.WithTree',
		element: () => import('./dashboard-with-tree.element.js'),
		weight: 3000,
		meta: {
			label: 'Tree Example',
			pathname: 'tree-example',
		},
	},
];
