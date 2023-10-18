import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

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
		type: 'sidebar',
		size: 'small',
	},
);
