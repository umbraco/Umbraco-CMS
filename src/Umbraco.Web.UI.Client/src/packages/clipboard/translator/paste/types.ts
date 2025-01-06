import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './clipboard-paste-translator.extension.js';

export interface UmbClipboardPasteTranslator<ClipboardEntryValueType = any, PropertyValueModelType = any>
	extends UmbApi {
	translate: (value: ClipboardEntryValueType) => Promise<PropertyValueModelType>;
}
