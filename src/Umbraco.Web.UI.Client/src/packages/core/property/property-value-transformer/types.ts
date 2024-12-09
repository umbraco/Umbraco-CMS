import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-transformer.extension.js';

export interface UmbPropertyValueTransformer<ValueType> extends UmbApi {
	transformValue?: UmbPropertyValueTransformerMethod<ValueType>;
}

export type UmbPropertyValueTransformerMethod<ValueType> = (value: ValueType) => PromiseLike<ValueType | undefined>;
