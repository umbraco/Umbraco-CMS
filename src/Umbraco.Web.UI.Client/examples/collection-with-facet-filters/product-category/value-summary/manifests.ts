import { EXAMPLE_PRODUCT_CATEGORY_REFERENCE_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Example.ValueSummary.ProductCategory.Reference',
		name: 'Product Category Reference Value Summary',
		forValueType: EXAMPLE_PRODUCT_CATEGORY_REFERENCE_VALUE_TYPE,
		element: () => import('./product-category-value-summary.element.js'),
		valueResolver: () => import('./product-category-value-summary.resolver.js'),
	},
];
