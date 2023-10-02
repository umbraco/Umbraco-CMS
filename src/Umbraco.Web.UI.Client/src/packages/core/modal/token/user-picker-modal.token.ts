import { UmbUserDetail } from '@umbraco-cms/backoffice/user';
import { UmbModalToken, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';

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
