import type { UserDetails } from '@umbraco-cms/models';
import { UmbModalToken, UmbPickerModalData } from '@umbraco-cms/modal';

export const UMB_USER_GROUP_PICKER_MODAL_TOKEN = new UmbModalToken<UmbPickerModalData<UserDetails>>(
	'Umb.Modal.UserGroupPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
