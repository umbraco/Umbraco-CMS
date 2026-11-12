export const EXAMPLE_PRODUCT_COLOR_REFERENCES_VALUE_TYPE = 'Example.ValueType.ProductColor.References' as const;

declare global {
	interface UmbValueTypeMap {
		[EXAMPLE_PRODUCT_COLOR_REFERENCES_VALUE_TYPE]: Array<string>;
	}
}
