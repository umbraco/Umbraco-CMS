import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';
import type { UmbDocumentItemModel } from '@umbraco-cms/backoffice/document';

export type UmbDocumentPickerModalData = UmbTreePickerModalData<UmbDocumentItemModel>;
export type UmbDocumentPickerModalValue = UmbPickerModalValue;

export const UMB_DOCUMENT_PICKER_MODAL = new UmbModalToken<UmbDocumentPickerModalData, UmbDocumentPickerModalValue>(
	'Umb.Modal.TreePicker',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
		data: {
			treeAlias: 'Umb.Tree.Document',
		},
	},
);
