import { EXAMPLE_PRODUCT_PRICE_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Example.ValueSummary.ProductPrice',
		name: 'Product Price Value Summary',
		forValueType: EXAMPLE_PRODUCT_PRICE_VALUE_TYPE,
		element: () => import('./product-price-value-summary.element.js'),
	},
];
