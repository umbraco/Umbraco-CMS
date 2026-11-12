import { EXAMPLE_PRODUCT_COLOR_REFERENCES_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Example.ValueSummary.ProductColor.References',
		name: 'Product Color References Value Summary',
		forValueType: EXAMPLE_PRODUCT_COLOR_REFERENCES_VALUE_TYPE,
		element: () => import('./product-color-value-summary.element.js'),
		valueResolver: () => import('./product-color-value-summary.resolver.js'),
	},
];
