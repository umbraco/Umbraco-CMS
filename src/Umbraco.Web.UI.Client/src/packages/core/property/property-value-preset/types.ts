import type { UmbPropertyEditorConfig } from '@umbraco-cms/backoffice/property-editor';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-value-preset.extension.js';

export interface UmbPropertyValuePresetApi<
	ValueType = unknown,
	ConfigType extends UmbPropertyEditorConfig = UmbPropertyEditorConfig,
> extends UmbApi {
	processValue: UmbPropertyValuePresetApiValuesProcessor<ValueType, ConfigType>;
}

export type UmbPropertyValuePresetApiValuesProcessor<
	ValueType = unknown,
	ConfigType extends UmbPropertyEditorConfig = UmbPropertyEditorConfig,
> = (value: ValueType, config: ConfigType) => PromiseLike<ValueType | undefined>;

export interface UmbPropertyTypePresetModel {
	alias: string;
	propertyEditorUiAlias: string;
	config: UmbPropertyEditorConfig;
}

export interface UmbPropertyTypePresetWithSchemaAliasModel extends UmbPropertyTypePresetModel {
	propertyEditorSchemaAlias: string;
}
