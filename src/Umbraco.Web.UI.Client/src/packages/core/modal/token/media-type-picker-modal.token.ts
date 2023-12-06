import { EntityTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbMediaTypePickerModalData = UmbTreePickerModalData<EntityTreeItemResponseModel>;
export type UmbMediaTypePickerModalValue = UmbPickerModalValue;

export const UMB_MEDIA_TYPE_PICKER_MODAL = new UmbModalToken<UmbMediaTypePickerModalData, UmbMediaTypePickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		config: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.MediaType',
		},
	},
);
