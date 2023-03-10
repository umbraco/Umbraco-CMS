import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbSectionPickerModalData {
	multiple: boolean;
	selection: string[];
}

export const UMB_SECTION_PICKER_MODAL_TOKEN = new UmbModalToken<UmbSectionPickerModalData>('Umb.Modal.SectionPicker', {
	type: 'sidebar',
	size: 'small',
});
