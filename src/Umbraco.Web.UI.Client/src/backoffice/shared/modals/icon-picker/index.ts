import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbIconPickerModalData {
	multiple: boolean;
	selection: string[];
}

export interface UmbIconPickerModalResult {
	color: string | undefined;
	icon: string | undefined;
}

export const UMB_ICON_PICKER_MODAL_TOKEN = new UmbModalToken<UmbIconPickerModalData, UmbIconPickerModalResult>(
	'Umb.Modal.IconPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
