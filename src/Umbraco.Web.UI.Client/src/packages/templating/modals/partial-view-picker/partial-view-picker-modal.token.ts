import { UMB_PARTIAL_VIEW_TREE_ALIAS } from '../../partial-views/tree/manifests.js';
import { UmbModalToken, type UmbPickerModalValue, type UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import type { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type UmbPartialViewPickerModalData = UmbTreePickerModalData<UmbEntityTreeItemModel>;
export type UmbPartialViewPickerModalValue = UmbPickerModalValue;

export const UMB_PARTIAL_VIEW_PICKER_MODAL = new UmbModalToken<
	UmbPartialViewPickerModalData,
	UmbPartialViewPickerModalValue
>('Umb.Modal.TreePicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},

	data: { treeAlias: UMB_PARTIAL_VIEW_TREE_ALIAS, hideTreeRoot: true },
});
