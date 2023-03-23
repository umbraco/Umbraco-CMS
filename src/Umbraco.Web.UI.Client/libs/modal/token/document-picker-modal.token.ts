import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentPickerModalData {
	multiple?: boolean;
	selection?: Array<string>;
}

export interface UmbDocumentPickerModalResult {
	selection: Array<string>;
}

export const UMB_DOCUMENT_PICKER_MODAL = new UmbModalToken<UmbDocumentPickerModalData, UmbDocumentPickerModalResult>(
	'Umb.Modal.DocumentPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
