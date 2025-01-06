import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbClipboardEntryValueModel<ValueType = any> {
	type: string;
	value: ValueType;
}

export type UmbClipboardEntryValuesType = Array<UmbClipboardEntryValueModel>;

export interface UmbClipboardCopyTranslator<PropertyValueModelType = any, ClipboardEntryValueType = any>
	extends UmbApi {
	translate: (propertyValue: PropertyValueModelType) => Promise<ClipboardEntryValueType>;
}

export interface UmbClipboardPasteTranslator<ClipboardEntryValueType = any, PropertyValueModelType = any>
	extends UmbApi {
	translate: (value: ClipboardEntryValueType) => Promise<PropertyValueModelType>;
}
