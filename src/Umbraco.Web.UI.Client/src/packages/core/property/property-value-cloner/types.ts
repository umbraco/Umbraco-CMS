import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-cloner.extension.js';

export interface UmbPropertyValueCloner<ValueType> extends UmbApi {
	cloneValue?: UmbPropertyValueClonerMethod<ValueType>;
}

export type UmbPropertyValueClonerMethod<ValueType> = (value: ValueType) => PromiseLike<ValueType | undefined>;
