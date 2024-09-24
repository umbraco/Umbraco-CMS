import type { UmbEntityVariantModel } from '../../variant/types.js';
import type { UmbPropertyValueData } from '../types/index.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-resolver.extension.js';

export interface UmbPropertyValueResolver<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	InnerPropertyValueType extends UmbPropertyValueData = PropertyValueType,
	InnerVariantModelType extends UmbEntityVariantModel = UmbEntityVariantModel,
> extends UmbApi {
	processValues?: UmbPropertyValueResolverValueProcessor<PropertyValueType, InnerPropertyValueType>;
	processVariants?: UmbPropertyValueResolverVariantProcessor<PropertyValueType, InnerVariantModelType>;
}

export type UmbPropertyValueResolverValueProcessor<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	InnerPropertyValueType extends UmbPropertyValueData = PropertyValueType,
> = (
	value: PropertyValueType,
	valuesProcessor: (values: Array<InnerPropertyValueType>) => Promise<Array<InnerPropertyValueType> | undefined>,
) => PromiseLike<PropertyValueType | undefined>;

export type UmbPropertyValueResolverVariantProcessor<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	VariantType extends UmbEntityVariantModel = UmbEntityVariantModel,
> = (
	value: PropertyValueType,
	variantsProcessor: (variants: Array<VariantType>) => Promise<Array<VariantType> | undefined>,
) => PromiseLike<PropertyValueType | undefined>;
