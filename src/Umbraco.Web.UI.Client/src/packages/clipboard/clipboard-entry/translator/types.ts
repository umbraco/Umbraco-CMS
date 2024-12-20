import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCopyClipboardEntryTranslator<PropertyValueModelType = any, ClipboardEntryModelType = any>
	extends UmbApi {
	translate: (propertyValue: PropertyValueModelType) => Promise<ClipboardEntryModelType | undefined>;
}

export interface UmbPasteClipboardEntryTranslator<ClipboardEntryModelType = any, PropertyValueModelType = any>
	extends UmbApi {
	translate: (clipboardEntry: ClipboardEntryModelType) => Promise<PropertyValueModelType | undefined>;
}
