import type { UserDetails } from '@umbraco-cms/backoffice/models';
import { UmbModalToken, UmbPickerModalData } from '@umbraco-cms/backoffice/modal';

export const UMB_USER_GROUP_PICKER_MODAL = new UmbModalToken<UmbPickerModalData<UserDetails>>(
	'Umb.Modal.UserGroupPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
