import { UMB_DOCUMENT_VARIANT_PICKER_MODAL_ALIAS } from '../manifests.js';
import type { UmbDocumentVariantModel } from '../../types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentVariantPickerModalData {
	type: 'save' | 'publish' | 'schedule' | 'unpublish';
	variants: Array<UmbDocumentVariantModel>;
}

export interface UmbDocumentVariantPickerModalValue {
	selection: Array<string | null>;
}

export const UMB_DOCUMENT_LANGUAGE_PICKER_MODAL = new UmbModalToken<
	UmbDocumentVariantPickerModalData,
	UmbDocumentVariantPickerModalValue
>(UMB_DOCUMENT_VARIANT_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
