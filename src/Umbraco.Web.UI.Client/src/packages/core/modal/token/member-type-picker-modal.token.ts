import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type UmbMemberTypePickerModalData = UmbTreePickerModalData<UmbEntityTreeItemModel>;
export type UmbMemberTypePickerModalValue = UmbPickerModalValue;

export const UMB_MEMBER_TYPE_PICKER_MODAL = new UmbModalToken<UmbMemberTypePickerModalData, UmbMemberTypePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.MemberType',
		},
	},
);
