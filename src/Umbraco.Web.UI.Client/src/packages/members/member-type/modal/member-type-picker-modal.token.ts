import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UMB_TREE_PICKER_MODAL_ALIAS } from '@umbraco-cms/backoffice/tree';
import type { UmbTreePickerModalValue, UmbTreePickerModalData, UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type UmbMemberTypePickerModalData = UmbTreePickerModalData<UmbTreeItemModel>;
export type UmbMemberTypePickerModalValue = UmbTreePickerModalValue;

export const UMB_MEMBER_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbMemberTypePickerModalData,
	UmbMemberTypePickerModalValue
>(UMB_TREE_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: 'Umb.Tree.MemberType',
	},
});
