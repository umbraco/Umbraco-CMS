import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbClipboardEntryCopyTranslator<PropertyValueModelType = any, ClipboardEntryModelType = any>
	extends UmbApi {
	translate: (propertyValue: PropertyValueModelType) => Promise<ClipboardEntryModelType | undefined>;
}

export interface UmbClipboardEntryPasteTranslator<ClipboardEntryModelType = any, PropertyValueModelType = any>
	extends UmbApi {
	translate: (clipboardEntry: ClipboardEntryModelType) => Promise<PropertyValueModelType | undefined>;
}
