import type { UmbVariantId } from '../../variant/variant-id.class.js';
import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-preset.extension.js';

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
export interface UmbPropertyValuePresetApiCallArgs {
	variantId?: UmbVariantId;
}

export interface UmbPropertyTypePresetWithSchemaAliasModel extends UmbPropertyTypePresetModel {
	propertyEditorSchemaAlias: string;
}
