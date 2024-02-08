import { UMB_DOCUMENT_LANGUAGE_PICKER_MODAL_ALIAS } from '../manifests.js';
import type { UmbLanguageItemModel } from '@umbraco-cms/backoffice/language';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentLanguagePickerModalData {
	filter?: (language: UmbLanguageItemModel) => boolean;
	type: 'save' | 'publish' | 'schedule' | 'unpublish';
}

export interface UmbDocumentLanguagePickerModalValue {
	selection: Array<string | null>;
}

export const UMB_DOCUMENT_LANGUAGE_PICKER_MODAL = new UmbModalToken<
	UmbDocumentLanguagePickerModalData,
	UmbDocumentLanguagePickerModalValue
>(UMB_DOCUMENT_LANGUAGE_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
		size: 'small',
	},
});
