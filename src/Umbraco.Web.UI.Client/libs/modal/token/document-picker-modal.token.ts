import { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbPickerModalResult, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbDocumentPickerModalData = UmbTreePickerModalData<DocumentTreeItemResponseModel>;
export type UmbDocumentPickerModalResult = UmbPickerModalResult;

export const UMB_DOCUMENT_PICKER_MODAL = new UmbModalToken<UmbDocumentPickerModalData, UmbDocumentPickerModalResult>(
	'Umb.Modal.TreePicker',
	{
		type: 'sidebar',
		size: 'small',
	},
	{
		treeAlias: 'Umb.Tree.Documents',
	}
);
