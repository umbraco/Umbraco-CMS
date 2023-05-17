import { UmbModalToken } from 'src/libs/modal';

export interface UmbSectionPickerModalData {
	multiple: boolean;
	selection: Array<string | null>;
}

export interface UmbSectionPickerModalResult {
	selection: Array<string | null>;
}

export const UMB_SECTION_PICKER_MODAL = new UmbModalToken<UmbSectionPickerModalData, UmbSectionPickerModalResult>(
	'Umb.Modal.SectionPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
