export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		kind: 'default',
		name: 'Example Dashboard With Facet Filters',
		alias: 'Example.Dashboard.WithFacetFilters',
		element: () => import('./dashboard.element.js'),
		weight: 3100,
		meta: {
			label: 'Facet Filters Example',
			pathname: 'facet-filters-example',
		},
	},
];
