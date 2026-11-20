import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-preset.extension.js';

/**
 * Produces the initial value of a property.
 *
 * One instance is shared by every value generated for a property, such as one per variant, and `processValue` is
 * called for those values concurrently. An implementation must therefore be stateless between calls.
 */
export interface UmbPropertyValuePreset<
	ValueType = unknown,
	ConfigType extends UmbPropertyEditorConfig = UmbPropertyEditorConfig,
> extends UmbApi {
	processValue: UmbPropertyValuePresetValuesProcessor<ValueType, ConfigType>;
}

export type UmbPropertyValuePresetValuesProcessor<
	ValueType = unknown,
	ConfigType extends UmbPropertyEditorConfig = UmbPropertyEditorConfig,
> = (
	value: undefined | ValueType,
	config: ConfigType,
	typeArgs: UmbPropertyTypePresetModelTypeModel,
	callArgs: UmbPropertyValuePresetApiCallArgs,
) => PromiseLike<ValueType>;

export interface UmbPropertyTypePresetModel {
	alias: string;
	propertyEditorUiAlias: string;
	config: UmbPropertyEditorConfig;
	typeArgs: UmbPropertyTypePresetModelTypeModel;
}

export interface UmbPropertyTypePresetModelTypeModel {
	isMandatory?: boolean;
	variesByCulture?: boolean;
	variesBySegment?: boolean;
}

export interface UmbPropertyValuePresetApiCallArgsEntityBase {
	entityType: string;
	entityUnique: string;
	entityTypeUnique?: string;
}

export interface UmbPropertyValuePresetApiCallArgs extends UmbPropertyValuePresetApiCallArgsEntityBase {
	alias: string;
	propertyEditorUiAlias: string;
	propertyEditorSchemaAlias?: string;
	variantId?: UmbVariantId;
	value?: unknown;
}

export interface UmbPropertyTypePresetWithSchemaAliasModel extends UmbPropertyTypePresetModel {
	propertyEditorSchemaAlias: string;
}
