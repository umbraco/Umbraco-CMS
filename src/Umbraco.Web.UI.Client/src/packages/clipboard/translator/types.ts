import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbClipboardEntryValueModel {
	type: string;
	value: any;
}

export type UmbClipboardEntryValues = Array<UmbClipboardEntryValueModel>;

export interface UmbClipboardCopyTranslator<PropertyValueModelType = any> extends UmbApi {
	translate: (propertyValue: PropertyValueModelType) => Promise<Array<UmbClipboardEntryValueModel> | undefined>;
}

export interface UmbClipboardPasteTranslator<ClipboardEntryModelType = any, PropertyValueModelType = any>
	extends UmbApi {
	translate: (clipboardEntry: ClipboardEntryModelType) => Promise<PropertyValueModelType | undefined>;
}
