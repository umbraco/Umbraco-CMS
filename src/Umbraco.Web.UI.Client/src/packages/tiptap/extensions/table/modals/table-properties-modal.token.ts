import { UMB_TIPTAP_TABLE_PROPERTIES_MODAL_ALIAS } from './constants.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbPropertyValueData } from '@umbraco-cms/backoffice/property';

export type UmbTiptapTablePropertiesModalData = Record<string, unknown>;

export type UmbTiptapTablePropertiesModalValue = Array<UmbPropertyValueData>;

export const UMB_TIPTAP_TABLE_PROPERTIES_MODAL = new UmbModalToken<
	UmbTiptapTablePropertiesModalData,
	UmbTiptapTablePropertiesModalValue
>(UMB_TIPTAP_TABLE_PROPERTIES_MODAL_ALIAS, {
	modal: { size: 'small', type: 'sidebar' },
});
