import type { UmbMediaItemModel } from '../../repository/types.js';
import type { UmbTreePickerModalData } from '@umbraco-cms/backoffice/tree';
import { UmbModalToken, type UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export type UmbMediaPickerModalData = UmbTreePickerModalData<UmbMediaItemModel>;
export type UmbMediaPickerModalValue = UmbPickerModalValue;

export const UMB_MEDIA_PICKER_MODAL = new UmbModalToken<UmbMediaPickerModalData, UmbMediaPickerModalValue>(
	'Umb.Modal.MediaPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'medium',
		},
	},
);
