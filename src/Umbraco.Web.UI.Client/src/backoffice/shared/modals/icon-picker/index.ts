import { UmbModalToken } from 'libs/modal';

export interface UmbModalDocumentPickerData {
	multiple?: boolean;
	selection?: Array<string>;
}

export interface UmbModalDocumentPickerResponse {
	selection: Array<string>;
}

export const UMB_DOCUMENT_PICKER_MODAL_TOKEN = new UmbModalToken<UmbModalDocumentPickerData>(
	'Umb.Modal.DocumentPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
