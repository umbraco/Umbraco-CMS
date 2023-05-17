import { UmbModalToken, UmbPickerModalData } from 'src/packages/core/modal';

export const UMB_USER_GROUP_PICKER_MODAL = new UmbModalToken<UmbPickerModalData<any>>('Umb.Modal.UserGroupPicker', {
	type: 'sidebar',
	size: 'small',
});
