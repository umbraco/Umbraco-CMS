import { UmbModalToken, UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { ContentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type UmbMediaTreePickerModalData = UmbTreePickerModalData<ContentTreeItemResponseModel>;
export type UmbMediaTreePickerModalValue = UmbPickerModalValue;

export const UMB_MEDIA_TREE_PICKER_MODAL = new UmbModalToken<UmbMediaTreePickerModalData, UmbMediaTreePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		type: 'sidebar',
		size: 'small',
	},
	{
		treeAlias: 'Umb.Tree.Media',
	},
);
