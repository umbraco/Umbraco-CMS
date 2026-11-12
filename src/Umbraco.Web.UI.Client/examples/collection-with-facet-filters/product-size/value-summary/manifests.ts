import { EXAMPLE_PRODUCT_SIZE_REFERENCES_VALUE_TYPE } from '../value-type/constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'valueSummary',
		kind: 'default',
		alias: 'Example.ValueSummary.ProductSize.References',
		name: 'Product Size References Value Summary',
		forValueType: EXAMPLE_PRODUCT_SIZE_REFERENCES_VALUE_TYPE,
		element: () => import('./product-size-value-summary.element.js'),
		valueResolver: () => import('./product-size-value-summary.resolver.js'),
	},
];
