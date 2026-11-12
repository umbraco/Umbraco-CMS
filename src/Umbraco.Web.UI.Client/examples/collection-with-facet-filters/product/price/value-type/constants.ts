import type { ExampleProductPriceModel } from '../types.js';

export const EXAMPLE_PRODUCT_PRICE_VALUE_TYPE = 'Example.ValueType.ProductPrice' as const;

declare global {
	interface UmbValueTypeMap {
		[EXAMPLE_PRODUCT_PRICE_VALUE_TYPE]: ExampleProductPriceModel;
	}
}
