import { UmbModalToken, UmbPickerModalValue, UmbTreePickerModalData } from '@umbraco-cms/backoffice/modal';
import { UmbEntityTreeItemModel } from '@umbraco-cms/backoffice/tree';

export type UmbDocumentTypePickerModalData = UmbTreePickerModalData<UmbEntityTreeItemModel>;
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
