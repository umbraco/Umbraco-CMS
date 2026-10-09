import type { UmbPropertyEditorRteValueType } from '@umbraco-cms/backoffice/rte';

export const UMB_TIPTAP_PROPERTY_EDITOR_VALUE_TYPE = 'Umbraco.RichText' as const;

declare global {
	interface UmbValueTypeMap {
		[UMB_TIPTAP_PROPERTY_EDITOR_VALUE_TYPE]: UmbPropertyEditorRteValueType;
	}
}
