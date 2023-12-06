import { UmbModalToken, UmbPickerModalData, UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export const UMB_USER_GROUP_PICKER_MODAL = new UmbModalToken<UmbPickerModalData<any>, UmbPickerModalValue>(
	'Umb.Modal.UserGroupPicker',
	{
		config: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
