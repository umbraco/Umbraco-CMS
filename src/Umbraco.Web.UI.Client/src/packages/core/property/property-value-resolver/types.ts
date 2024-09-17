import type { UmbPropertyValueData } from '../types/index.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-resolver.extension.js';

export interface UmbPropertyValueResolver<PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData>
	extends UmbApi {
	process: UmbPropertyValueResolverProcessMethod<PropertyValueType> | undefined;
}

export type UmbPropertyValueResolverProcessMethod<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
> = (value: PropertyValueType, valueProcessor: (value: PropertyValueType) => PropertyValueType) => PropertyValueType;
