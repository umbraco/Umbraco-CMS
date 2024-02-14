import { UmbUniqueTreeItemModel } from '@umbraco-cms/backoffice/tree';
import { UmbModalToken } from '../../../core/modal/token/modal-token.js';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbMemberTypePickerModalData = UmbTreePickerModalData<UmbUniqueTreeItemModel>;
export type UmbMemberTypePickerModalValue = UmbPickerModalValue;

export const UMB_MEMBER_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbMemberTypePickerModalData,
	UmbMemberTypePickerModalValue
>('Umb.Modal.TreePicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: 'Umb.Tree.MemberType',
	},
});
