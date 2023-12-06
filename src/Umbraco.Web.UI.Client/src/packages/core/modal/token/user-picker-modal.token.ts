import { UmbUserDetailModel } from '@umbraco-cms/backoffice/user';
import { UmbModalToken, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';

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
