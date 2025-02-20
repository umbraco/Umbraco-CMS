import { UmbModalToken } from '../../token/index.js';

export type UmbItemPickerModalData = {
	headline: string;
	items: Array<UmbItemPickerModel>;
};

export type UmbItemPickerModel = {
	description?: string;
	icon?: string;
	label: string;
	value: string;
};

export const UMB_ITEM_PICKER_MODAL = new UmbModalToken<UmbItemPickerModalData, UmbItemPickerModel>(
	'Umb.Modal.ItemPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
