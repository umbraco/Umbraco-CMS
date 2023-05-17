import { EntityTreeItemResponseModel } from 'src/libs/backend-api';
import { UmbModalToken, UmbPickerModalResult, UmbTreePickerModalData } from 'src/libs/modal';

export type UmbDocumentTypePickerModalData = UmbTreePickerModalData<EntityTreeItemResponseModel>;
export type UmbDocumentTypePickerModalResult = UmbPickerModalResult;

export const UMB_DOCUMENT_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbDocumentTypePickerModalData,
	UmbDocumentTypePickerModalResult
>(
	'Umb.Modal.TreePicker',
	{
		type: 'sidebar',
		size: 'small',
	},
	{
		treeAlias: 'Umb.Tree.DocumentTypes',
	}
);
