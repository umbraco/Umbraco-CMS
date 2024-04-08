import { UMB_PARTIAL_VIEW_TREE_ALIAS } from '../../partial-views/tree/manifests.js';
import type { UmbPartialViewTreeItemModel } from '../../partial-views/tree/index.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbPartialViewPickerModalData = UmbTreePickerModalData<UmbPartialViewTreeItemModel>;
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
