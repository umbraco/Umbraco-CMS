import { UmbModalToken } from '../../modal/token/modal-token.js';

export interface UmbSectionPickerModalData {
	multiple: boolean;
	selection: Array<string | null>;
}

export interface UmbSectionPickerModalValue {
	selection: Array<string | null>;
}

export const UMB_SECTION_PICKER_MODAL = new UmbModalToken<UmbSectionPickerModalData, UmbSectionPickerModalValue>(
	'Umb.Modal.SectionPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
