import type { UmbDocumentVariantPickerData, UmbDocumentVariantPickerValue } from '../../../modals/types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_UNPUBLISH_MODAL_ALIAS = 'Umb.Modal.DocumentUnpublish';

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
