import { UmbModalToken } from './modal-token.js';

export interface UmbListitemPickerItem {
	key: string;
	name: string;
	selected?: boolean;
}

export interface UmbListitemPickerModalData {
	headline: string;
	items: UmbListitemPickerItem[];
	confirmLabel?: string;
}

export type UmbListitemPickerModalValue = UmbListitemPickerItem['key'][] | undefined;

export const UMB_LISTITEM_PICKER_MODAL = new UmbModalToken<UmbListitemPickerModalData, UmbListitemPickerModalValue>(
	'Umb.Modal.ListitemPicker',
	{
		modal: {
			type: 'dialog',
		},
	},
);
