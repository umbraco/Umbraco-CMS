import { UMB_PARTIAL_VIEW_TREE_ALIAS } from '../../partial-views/tree/manifests.js';
import type { UmbPartialViewTreeItemModel } from '../types.js';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbTreePickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/tree';
import { UMB_TREE_PICKER_MODAL_ALIAS } from '@umbraco-cms/backoffice/tree';

export type UmbPartialViewPickerModalData = UmbTreePickerModalData<UmbPartialViewTreeItemModel>;
export type UmbPartialViewPickerModalValue = UmbTreePickerModalValue;

export const UMB_PARTIAL_VIEW_PICKER_MODAL = new UmbModalToken<
	UmbPartialViewPickerModalData,
	UmbPartialViewPickerModalValue
>(UMB_TREE_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: { treeAlias: UMB_PARTIAL_VIEW_TREE_ALIAS, hideTreeRoot: true },
});
