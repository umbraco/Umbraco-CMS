import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbPickerModalResult, UmbTreePickerModalData } from 'src/packages/core/modal';

export type UmbTemplatePickerModalData = UmbTreePickerModalData<EntityTreeItemResponseModel>;
export type UmbTemplatePickerModalResult = UmbPickerModalResult;

export const UMB_TEMPLATE_PICKER_MODAL = new UmbModalToken<UmbTemplatePickerModalData, UmbTemplatePickerModalResult>(
	'Umb.Modal.TreePicker',
	{
		type: 'sidebar',
		size: 'small',
	},
	{
		treeAlias: 'Umb.Tree.Templates',
	}
);
