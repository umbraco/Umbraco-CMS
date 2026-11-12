export const EXAMPLE_PRODUCT_CATEGORY_REFERENCE_VALUE_TYPE = 'Example.ValueType.ProductCategory.Reference' as const;

declare global {
	interface UmbValueTypeMap {
		[EXAMPLE_PRODUCT_CATEGORY_REFERENCE_VALUE_TYPE]: string;
	}
}
