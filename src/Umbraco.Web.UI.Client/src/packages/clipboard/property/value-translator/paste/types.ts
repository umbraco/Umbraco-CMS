import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './clipboard-paste-translator.extension.js';

export interface UmbClipboardPastePropertyValueTranslator<
	ClipboardEntryValueType = any,
	PropertyValueType = any,
	ConfigType = any,
> extends UmbApi {
	translate: (value: ClipboardEntryValueType) => Promise<PropertyValueType>;
	isCompatibleValue?: (
		value: ClipboardEntryValueType,
		config: ConfigType,
		filter?: (value: ClipboardEntryValueType, config: ConfigType) => Promise<boolean>,
	) => Promise<boolean>;
}
