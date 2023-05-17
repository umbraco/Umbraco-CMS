import { UmbModalToken, UmbPickerModalData } from 'src/libs/modal';
import { UserResponseModel } from 'src/libs/backend-api';

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
