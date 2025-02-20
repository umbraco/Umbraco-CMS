import type { UmbPropertyValueData } from '../types/index.js';
import type { UmbVariantDataModel } from '@umbraco-cms/backoffice/variant';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-resolver.extension.js';

export interface UmbPropertyValueResolver<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	InnerPropertyValueType extends UmbPropertyValueData = PropertyValueType,
	InnerVariantType extends UmbVariantDataModel = UmbVariantDataModel,
> extends UmbApi {
	processValues?: UmbPropertyValueResolverValuesProcessor<PropertyValueType, InnerPropertyValueType>;
	processVariants?: UmbPropertyValueResolverVariantsProcessor<PropertyValueType, InnerVariantType>;
	//ensureVariants?: UmbPropertyValueResolverEnsureVariants<PropertyValueType>;
	compareVariants?: (a: InnerVariantType, b: InnerVariantType) => boolean;
}

export type UmbPropertyValueResolverValuesProcessor<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	InnerPropertyValueType extends UmbPropertyValueData = PropertyValueType,
> = (
	value: PropertyValueType,
	valuesProcessor: (values: Array<InnerPropertyValueType>) => Promise<Array<InnerPropertyValueType> | undefined>,
) => PromiseLike<PropertyValueType | undefined>;

export type UmbPropertyValueResolverVariantsProcessor<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	InnerVariantType extends UmbVariantDataModel = UmbVariantDataModel,
> = (
	value: PropertyValueType,
	variantsProcessor: (values: Array<InnerVariantType>) => Promise<Array<InnerVariantType> | undefined>,
) => PromiseLike<PropertyValueType | undefined>;
