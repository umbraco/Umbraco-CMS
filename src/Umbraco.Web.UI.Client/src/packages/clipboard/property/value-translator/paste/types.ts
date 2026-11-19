import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './clipboard-paste-translator.extension.js';

export interface UmbClipboardPastePropertyValueTranslator<
	ClipboardEntryValueType = any,
	PropertyValueType = any,
	ConfigType = any,
> extends UmbApi {
	/**
	 * Turn a clipboard entry value into a value for the property editor this translator targets.
	 * @param clipboardEntryValue The value stored on the clipboard entry.
	 * @param config The configuration of the property being pasted into, so the result can be one the property
	 * editor actually supports. Arrives as the raw array of config properties, so look up by alias.
	 */
	translate: (clipboardEntryValue: ClipboardEntryValueType, config: ConfigType) => Promise<PropertyValueType>;
	isCompatibleValue?: (
		propertyValue: PropertyValueType,
		config: ConfigType,
		filter?: (propertyValue: PropertyValueType, config: ConfigType) => Promise<boolean>,
	) => Promise<boolean>;
}
