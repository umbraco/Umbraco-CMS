import type { UserDetails } from '@umbraco-cms/backoffice/models';
import { UmbModalToken, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';

export const UMB_USER_PICKER_MODAL = new UmbModalToken<UmbPickerModalData<UserDetails>>('Umb.Modal.UserPicker', {
	type: 'sidebar',
	size: 'small',
});
