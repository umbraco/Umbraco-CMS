import type { UmbMemberGroupItemModel } from '../../types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMemberGroupPickerModalData {
	multiple?: boolean;
	filter?: (memberGroup: UmbMemberGroupItemModel) => boolean;
}

export interface UmbMemberGroupPickerModalValue {
	selection: Array<string | null>;
}

export const UMB_MEMBER_GROUP_PICKER_MODAL = new UmbModalToken<
	UmbMemberGroupPickerModalData,
	UmbMemberGroupPickerModalValue
>('Umb.Modal.MemberGroupPicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
