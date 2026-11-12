export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'localization',
		alias: 'Example.Localization.ProductPriceFilter.En',
		name: 'Product Price Filter English Localization',
		js: () => import('./en.js'),
		weight: 1,
		meta: {
			culture: 'en',
		},
	},
];
