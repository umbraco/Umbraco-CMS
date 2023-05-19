import { UmbModalToken, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';
import { UserResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type UmbUserPickerModalData = UmbPickerModalData<UserResponseModel>;

export interface UmbUserPickerModalResult {
	selection: Array<string | null>;
}

export const UMB_USER_PICKER_MODAL = new UmbModalToken<UmbUserPickerModalData, UmbUserPickerModalResult>(
	'Umb.Modal.UserPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
