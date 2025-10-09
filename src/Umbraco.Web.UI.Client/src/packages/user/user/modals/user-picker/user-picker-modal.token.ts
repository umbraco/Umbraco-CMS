import type { UmbUserDetailModel } from '../../types.js';
import type { UmbPickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbUserPickerModalData = UmbPickerModalData<UmbUserDetailModel>;

export interface UmbUserPickerModalValue {
	selection: Array<string | null>;
}

export const UMB_USER_PICKER_MODAL = new UmbModalToken<UmbUserPickerModalData, UmbUserPickerModalValue>(
	'Umb.Modal.User.Picker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
