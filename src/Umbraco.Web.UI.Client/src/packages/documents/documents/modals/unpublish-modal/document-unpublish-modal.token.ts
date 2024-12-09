import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../types.js';
import { UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS } from './manifest.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentUnpublishModalData extends UmbDocumentVariantPickerData {
	documentUnique?: string;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentUnpublishModalValue extends UmbDocumentVariantPickerValue {}

export const UMB_DOCUMENT_UNPUBLISH_MODAL = new UmbModalToken<
	UmbDocumentUnpublishModalData,
	UmbDocumentUnpublishModalValue
>(UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
