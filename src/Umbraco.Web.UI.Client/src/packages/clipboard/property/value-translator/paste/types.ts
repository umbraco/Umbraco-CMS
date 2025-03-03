import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './clipboard-paste-translator.extension.js';

export interface UmbClipboardPastePropertyValueTranslator<
	ClipboardEntryValueType = any,
	PropertyValueModelType = any,
	ConfigType = any,
> extends UmbApi {
	translate: (value: ClipboardEntryValueType) => Promise<PropertyValueModelType>;
	isCompatibleValue?: (value: ClipboardEntryValueType, config: ConfigType) => Promise<boolean>;
}
