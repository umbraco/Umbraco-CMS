import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbDocumentTypePickerModalData {
	multiple?: boolean;
	selection?: Array<string>;
}

export interface UmbDocumentTypePickerModalResult {
	selection: Array<string>;
}

export const UMB_DOCUMENT_TYPE_PICKER_MODAL_TOKEN = new UmbModalToken<
	UmbDocumentTypePickerModalData,
	UmbDocumentTypePickerModalResult
>('Umb.Modal.DocumentTypePicker', {
	type: 'sidebar',
	size: 'small',
});
