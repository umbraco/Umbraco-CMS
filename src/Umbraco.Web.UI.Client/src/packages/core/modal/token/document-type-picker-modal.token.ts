import type { UmbDocumentTypeTreeItemModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbDocumentTypePickerModalData = UmbTreePickerModalData<UmbDocumentTypeTreeItemModel>;
export type UmbDocumentTypePickerModalValue = UmbPickerModalValue;

export const UMB_DOCUMENT_TYPE_PICKER_MODAL = new UmbModalToken<
	UmbDocumentTypePickerModalData,
	UmbDocumentTypePickerModalValue
>('Umb.Modal.TreePicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
	data: {
		treeAlias: 'Umb.Tree.DocumentType',
	},
});
