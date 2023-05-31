import { UmbModalToken, UmbPickerModalResult, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { ContentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';

export type UmbMediaTreePickerModalData = UmbTreePickerModalData<ContentTreeItemResponseModel>;
export type UmbMediaTreePickerModalResult = UmbPickerModalResult;

export const UMB_MEDIA_TREE_PICKER_MODAL = new UmbModalToken<
	UmbMediaTreePickerModalData,
	UmbMediaTreePickerModalResult
>(
	'Umb.Modal.TreePicker',
	{
		type: 'sidebar',
		size: 'small',
	},
	{
		treeAlias: 'Umb.Tree.Media',
	}
);
