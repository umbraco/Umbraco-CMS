import { UmbModalToken, UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type UmbMediaTypePickerModalData = UmbTreePickerModalData<UmbEntityTreeItemModel>;
export type UmbMediaTypePickerModalValue = UmbPickerModalValue;

export const UMB_MEDIA_TYPE_PICKER_MODAL = new UmbModalToken<UmbMediaTypePickerModalData, UmbMediaTypePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.MediaType',
		},
	},
);
