import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './clipboard-paste-translator.extension.js';

export interface UmbClipboardPastePropertyValueTranslator<
	ClipboardEntryValueType = any,
	PropertyValueType = any,
	ConfigType = any,
> extends UmbApi {
	translate: (clipboardEntryValue: ClipboardEntryValueType) => Promise<PropertyValueType>;
	isCompatibleValue?: (
		propertyValue: PropertyValueType,
		config: ConfigType,
		filter?: (propertyValue: PropertyValueType, config: ConfigType) => Promise<boolean>,
	) => Promise<boolean>;
}
