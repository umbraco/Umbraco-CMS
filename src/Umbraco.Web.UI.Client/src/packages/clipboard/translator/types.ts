import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbClipboardEntryValueModel<ValueType = any> {
	type: string;
	value: ValueType;
}

export type UmbClipboardEntryValuesType = Array<UmbClipboardEntryValueModel>;

export interface UmbClipboardCopyTranslator<PropertyValueModelType> extends UmbApi {
	translate: (propertyValue: PropertyValueModelType) => Promise<UmbClipboardEntryValuesType>;
}

export interface UmbClipboardPasteTranslator<PropertyValueModelType = unknown> extends UmbApi {
	translate: (clipboardEntryValue: UmbClipboardEntryValueModel) => Promise<PropertyValueModelType>;
}
