import type { UmbClipboardEntryDetailModel } from '../clipboard-entry/types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbClipboardCopyResolver<PropertyValueType = unknown, ClipboardValueType = unknown> extends UmbApi {
	resolve: (value: PropertyValueType) => Promise<ClipboardValueType>;
}

export interface UmbClipboardPasteResolver<
	ClipboardEntryType extends UmbClipboardEntryDetailModel = UmbClipboardEntryDetailModel,
> extends UmbApi {
	getAcceptedTypes: () => Promise<string[]>;
	resolve: (unique: string) => Promise<ClipboardEntryType | undefined>;
}
