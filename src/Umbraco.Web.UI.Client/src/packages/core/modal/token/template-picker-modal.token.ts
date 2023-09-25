import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbTemplatePickerModalData = UmbTreePickerModalData<EntityTreeItemResponseModel>;
export type UmbTemplatePickerModalValue = UmbPickerModalValue;

export const UMB_TEMPLATE_PICKER_MODAL = new UmbModalToken<UmbTemplatePickerModalData, UmbTemplatePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		type: 'sidebar',
		size: 'small',
	},
	{
		treeAlias: 'Umb.Tree.Templates',
	},
);
