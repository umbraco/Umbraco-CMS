import type { UmbMemberItemModel } from '@umbraco-cms/backoffice/member';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbMemberPickerModalData {
	multiple?: boolean;
	filter?: (member: UmbMemberItemModel) => boolean;
}

export interface UmbMemberPickerModalValue {
	selection: Array<string | null>;
}

export const UMB_MEMBER_PICKER_MODAL = new UmbModalToken<UmbMemberPickerModalData, UmbMemberPickerModalValue>(
	'Umb.Modal.MemberPicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
