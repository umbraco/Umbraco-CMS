import { UmbModalToken, UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type UmbTemplatePickerModalData = UmbTreePickerModalData<UmbEntityTreeItemModel>;
export type UmbTemplatePickerModalValue = UmbPickerModalValue;

export const UMB_TEMPLATE_PICKER_MODAL = new UmbModalToken<UmbTemplatePickerModalData, UmbTemplatePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.Templates',
		},
	},
);
