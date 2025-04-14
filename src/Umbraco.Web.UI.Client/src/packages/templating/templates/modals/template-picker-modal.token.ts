import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import { UMB_TREE_PICKER_MODAL_ALIAS } from '@umbraco-cms/backoffice/tree';
import type { UmbTreePickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/tree';
import type { UmbTemplateTreeItemModel } from '../tree/index.js';

export type UmbTemplatePickerModalData = UmbTreePickerModalData<UmbTemplateTreeItemModel>;
export type UmbTemplatePickerModalValue = UmbTreePickerModalValue;

export const UMB_TEMPLATE_PICKER_MODAL = new UmbModalToken<UmbTemplatePickerModalData, UmbTemplatePickerModalValue>(
	UMB_TREE_PICKER_MODAL_ALIAS,
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			hideTreeRoot: true,
			treeAlias: 'Umb.Tree.Template',
		},
	},
);
