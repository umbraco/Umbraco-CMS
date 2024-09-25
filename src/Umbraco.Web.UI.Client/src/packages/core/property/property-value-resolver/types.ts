import type { UmbPropertyValueData } from '../types/index.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-resolver.extension.js';

export interface UmbPropertyValueResolver<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	InnerPropertyValueType extends UmbPropertyValueData = PropertyValueType,
> extends UmbApi {
	processValues?: UmbPropertyValueResolverValueProcessor<PropertyValueType, InnerPropertyValueType>;
	ensureVariants?: UmbPropertyValueResolverEnsureVariants<PropertyValueType>;
}

export type UmbPropertyValueResolverValueProcessor<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
	InnerPropertyValueType extends UmbPropertyValueData = PropertyValueType,
> = (
	value: PropertyValueType,
	valuesProcessor: (values: Array<InnerPropertyValueType>) => Promise<Array<InnerPropertyValueType> | undefined>,
) => PromiseLike<PropertyValueType | undefined>;

export type UmbPropertyValueResolverEnsureVariants<
	PropertyValueType extends UmbPropertyValueData = UmbPropertyValueData,
> = (
	value: PropertyValueType,
	args: UmbPropertyValueResolverEnsureVariantArgs,
) => PromiseLike<PropertyValueType | undefined>;

export type UmbPropertyValueResolverEnsureVariantArgs = {
	selectedVariants: Array<UmbVariantId>;
};
