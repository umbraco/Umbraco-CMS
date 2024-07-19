import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMediaPickerModalData<ItemType> {
	startNode?: string | null;
	multiple?: boolean;
	pickableFilter?: (item: ItemType) => boolean;
	filter?: (item: ItemType) => boolean;
}

export interface UmbMediaPickerModalValue {
	selection: string[];
}

export const UMB_MEDIA_PICKER_MODAL = new UmbModalToken<UmbMediaPickerModalData<any>, UmbMediaPickerModalValue>(
	'Umb.Modal.MediaPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
