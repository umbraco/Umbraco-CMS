export const EXAMPLE_PRODUCT_SIZE_REFERENCES_VALUE_TYPE = 'Example.ValueType.ProductSize.References' as const;

declare global {
	interface UmbValueTypeMap {
		[EXAMPLE_PRODUCT_SIZE_REFERENCES_VALUE_TYPE]: Array<string>;
	}
}
