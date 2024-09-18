import type { UmbPropertyValueData } from '../types/index.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-resolver.extension.js';

export interface UmbPropertyValueResolver<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	InnerPropertyValueType extends UmbPropertyValueData = PropertyValueType,
> extends UmbApi {
	process: UmbPropertyValueResolverProcessMethod<PropertyValueType, InnerPropertyValueType> | undefined;
}

export type UmbPropertyValueResolverProcessMethod<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	InnerPropertyValueType extends UmbPropertyValueData = PropertyValueType,
> = (
	value: PropertyValueType,
	valueProcessor: (value: InnerPropertyValueType) => InnerPropertyValueType,
) => PropertyValueType | undefined;
