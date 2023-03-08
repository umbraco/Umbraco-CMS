import type { UserDetails } from '@umbraco-cms/models';
import { UmbModalToken } from 'libs/modal';
import type { UmbPickerModalData } from 'libs/modal/layouts/modal-layout-picker-base';

export const UMB_USER_PICKER_MODAL_TOKEN = new UmbModalToken<UmbPickerModalData<UserDetails>>('Umb.Modal.UserPicker', {
	type: 'sidebar',
	size: 'small',
});
