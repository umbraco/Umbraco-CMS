import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbIconPickerModalData {
	placeholder?: string;
	showEmptyOption?: boolean;
	hideColors?: boolean;
	colors?: Array<{ value: string, name: string }>;
}

export interface UmbIconPickerModalValue {
	color: string | undefined;
	icon: string | undefined;
}

export const UMB_ICON_PICKER_MODAL = new UmbModalToken<UmbIconPickerModalData, UmbIconPickerModalValue>(
	'Umb.Modal.IconPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
