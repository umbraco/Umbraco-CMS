import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbIconDefinition } from '../types.js';

export interface UmbIconPickerModalData {
	filter?: (item: UmbIconDefinition) => boolean;
	placeholder?: string;
	showEmptyOption?: boolean;
	hideColors?: boolean;
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
