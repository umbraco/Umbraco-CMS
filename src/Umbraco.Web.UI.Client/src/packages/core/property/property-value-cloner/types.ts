import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-cloner.extension.js';

export interface UmbPropertyValueCloner<ValueType> extends UmbApi {
	/**
	 * Clones a property value.
	 * @param value The value to clone.
	 * @returns A promise that resolves with the clonal(cloned value).
	 */
	cloneValue?: UmbPropertyValueClonerMethod<ValueType>;
}

/**
 * Clones a property value.
 * @param value The value to clone.
 * @returns A promise that resolves with the clonal(cloned value).
 */
export type UmbPropertyValueClonerMethod<ValueType> = (value: ValueType) => PromiseLike<ValueType | undefined>;
