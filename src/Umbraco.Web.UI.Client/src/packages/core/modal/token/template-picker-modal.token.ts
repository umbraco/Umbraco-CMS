import { UmbModalToken } from './modal-token.js';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import type { UmbTemplateTreeItemModel } from '@umbraco-cms/backoffice/template';

export type UmbTemplatePickerModalData = UmbTreePickerModalData<UmbTemplateTreeItemModel>;
export type UmbTemplatePickerModalValue = UmbPickerModalValue;

export const UMB_TEMPLATE_PICKER_MODAL = new UmbModalToken<UmbTemplatePickerModalData, UmbTemplatePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.Template',
		},
	},
);
