import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbPropertyEditorUIPickerModalData {
	/** @deprecated This property will be removed in Umbraco 15. */
	submitLabel?: string;
}

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
