import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbPropertyEditorUIPickerModalData = object;

export type UmbPropertyEditorUIPickerModalValue = {
	selection: Array<string>;
};

export const UMB_PROPERTY_EDITOR_UI_PICKER_MODAL = new UmbModalToken<
	UmbPropertyEditorUIPickerModalData,
	UmbPropertyEditorUIPickerModalValue
>('Umb.Modal.PropertyEditorUiPicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
