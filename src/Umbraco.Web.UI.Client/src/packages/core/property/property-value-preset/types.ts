import type { UmbPropertyValueData } from '../types/index.js';
import type { UmbVariantDataModel } from '@umbraco-cms/backoffice/variant';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-preset.extension.js';

export interface UmbPropertyValueResolver<PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData>
	extends UmbApi {
	processValue?: UmbPropertyValueResolverValuesProcessor<PropertyValueType>;
}

export type UmbPropertyValueResolverValuesProcessor<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
> = (value: PropertyValueType) => PromiseLike<PropertyValueType | undefined>;
