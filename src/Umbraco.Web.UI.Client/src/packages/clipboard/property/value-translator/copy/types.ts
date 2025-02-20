import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './clipboard-copy-translator.extension.js';

export interface UmbClipboardCopyPropertyValueTranslator<PropertyValueModelType = any, ClipboardEntryValueType = any>
	extends UmbApi {
	translate: (propertyValue: PropertyValueModelType) => Promise<ClipboardEntryValueType>;
}
