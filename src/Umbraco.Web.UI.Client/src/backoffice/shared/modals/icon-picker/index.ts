import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbIconPickerModalData {
	multiple: boolean;
	selection: string[];
}

export const UMB_ICON_PICKER_MODAL_TOKEN = new UmbModalToken<UmbIconPickerModalData>('Umb.Modal.IconPicker', {
	type: 'sidebar',
	size: 'small',
});
