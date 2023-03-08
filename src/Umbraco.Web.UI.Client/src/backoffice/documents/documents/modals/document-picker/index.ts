import { UmbModalToken } from 'libs/modal';

export interface UmbDocumentPickerModalData {
	multiple?: boolean;
	selection?: Array<string>;
}

export interface UmbDocumentPickerModalResponse {
	selection: Array<string>;
}

export const UMB_DOCUMENT_PICKER_MODAL_TOKEN = new UmbModalToken<UmbDocumentPickerModalData>(
	'Umb.Modal.DocumentPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
