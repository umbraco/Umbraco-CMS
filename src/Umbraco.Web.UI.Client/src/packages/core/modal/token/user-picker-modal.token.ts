import { UmbModalToken, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbUserDetail } from 'src/packages/user/user';

export type UmbUserPickerModalData = UmbPickerModalData<UmbUserDetail>;

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
