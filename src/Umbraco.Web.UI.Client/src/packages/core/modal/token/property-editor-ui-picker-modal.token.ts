import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbPropertyEditorUIPickerModalData {
	selection?: Array<string>;
	submitLabel?: string;
}

export type UmbPropertyEditorUIPickerModalResult = {
	selection: Array<string>;
};

export const UMB_PROPERTY_EDITOR_UI_PICKER_MODAL = new UmbModalToken<
	UmbPropertyEditorUIPickerModalData,
	UmbPropertyEditorUIPickerModalResult
>('Umb.Modal.PropertyEditorUiPicker', {
	type: 'sidebar',
	size: 'small',
});
