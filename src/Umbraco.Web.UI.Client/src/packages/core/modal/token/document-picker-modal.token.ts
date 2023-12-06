import { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbModalToken, UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';

export type UmbDocumentPickerModalData = UmbTreePickerModalData<DocumentTreeItemResponseModel>;
export type UmbDocumentPickerModalValue = UmbPickerModalValue;

export const UMB_DOCUMENT_PICKER_MODAL = new UmbModalToken<UmbDocumentPickerModalData, UmbDocumentPickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		config: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.Document',
		},
	},
);
