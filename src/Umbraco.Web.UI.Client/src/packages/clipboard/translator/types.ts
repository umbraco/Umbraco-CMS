import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbClipboardEntryValueModel {
	type: string;
	value: any;
}

export type UmbClipboardEntryValuesType = Array<UmbClipboardEntryValueModel>;

export interface UmbClipboardCopyTranslator<PropertyValueModelType = unknown> extends UmbApi {
	translate: (propertyValue: PropertyValueModelType) => Promise<UmbClipboardEntryValuesType>;
}

export interface UmbClipboardPasteTranslator<PropertyValueModelType = unknown> extends UmbApi {
	translate: (clipboardEntryValues: UmbClipboardEntryValuesType) => Promise<Array<PropertyValueModelType>>;
}
