import { UMB_DOCUMENT_VARIANT_PICKER_MODAL_ALIAS } from '../manifests.js';
import type { UmbDocumentVariantOptionModel } from '../../types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbDocumentVariantPickerModalType = 'save' | 'publish' | 'schedule' | 'unpublish';

export interface UmbDocumentVariantPickerModalData {
	type: UmbDocumentVariantPickerModalType;
	options: Array<UmbDocumentVariantOptionModel>;
}

export interface UmbDocumentVariantPickerModalValue {
	selection: Array<string>;
}

export const UMB_DOCUMENT_LANGUAGE_PICKER_MODAL = new UmbModalToken<
	UmbDocumentVariantPickerModalData,
	UmbDocumentVariantPickerModalValue
>(UMB_DOCUMENT_VARIANT_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
