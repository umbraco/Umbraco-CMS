import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbIconPickerModalData {
	color: string | undefined;
	icon: string | undefined;
}

export interface UmbIconPickerModalValue {
	color: string | undefined;
	icon: string | undefined;
}

export const UMB_ICON_PICKER_MODAL = new UmbModalToken<UmbIconPickerModalData, UmbIconPickerModalValue>(
	'Umb.Modal.IconPicker',
	{
		type: 'sidebar',
		size: 'small',
	},
);
