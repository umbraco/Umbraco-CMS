import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbClipboardCopyResolver<PropertyValueType = unknown> extends UmbApi {
	copy: (value: PropertyValueType, name: any, meta: any) => Promise<void>;
}

export interface UmbClipboardPasteResolver extends UmbApi {
	getAcceptedTypes: () => Promise<string[]>;
	resolve: (unique: string) => Promise<unknown | undefined>;
}
