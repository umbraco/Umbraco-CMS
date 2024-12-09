import type { UmbPropertyValueData } from '../types/index.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-transformer.extension.js';

export interface UmbPropertyValueTransformer<PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData>
	extends UmbApi {
	transform?: UmbPropertyValueTransformerMethod<PropertyValueType>;
}

export type UmbPropertyValueTransformerMethod<PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData> = (
	value: PropertyValueType,
) => PromiseLike<PropertyValueType | undefined>;
