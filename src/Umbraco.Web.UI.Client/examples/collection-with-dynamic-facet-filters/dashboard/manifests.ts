export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'dashboard',
		kind: 'default',
		name: 'Example Dynamic Facet Filters Dashboard',
		alias: 'Example.Dashboard.DynamicFacetFilters',
		element: () => import('./dashboard.element.js'),
		weight: 3100,
		meta: {
			label: 'Dynamic Facet Filters',
			pathname: 'dynamic-facet-filters',
		},
	},
];
