import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbClipboardCopyResolverCopyArgs<PropertyValueType = any, MetaType = object | undefined> {
	icon?: string;
	meta: MetaType;
	name: string;
	propertyValue: PropertyValueType;
}

export interface UmbClipboardCopyResolver<PropertyValueType = any, MetaType = object | undefined> extends UmbApi {
	copy: (args: UmbClipboardCopyResolverCopyArgs<PropertyValueType, MetaType>) => Promise<void>;
}

export interface UmbClipboardPasteResolver<PropertyValueType = any> extends UmbApi {
	getAcceptedTypes: () => Promise<string[]>;
	resolve: (unique: string) => Promise<PropertyValueType | undefined>;
}
