import { UMB_DOCUMENT_VARIANT_PICKER_MODAL_ALIAS } from '../manifests.js';
import type { UmbDocumentVariantModel } from '../../types.js';
import type { UmbLanguageItemModel } from '@umbraco-cms/backoffice/language';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentVariantPickerModalData {
	filter?: (language: UmbLanguageItemModel) => boolean;
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
