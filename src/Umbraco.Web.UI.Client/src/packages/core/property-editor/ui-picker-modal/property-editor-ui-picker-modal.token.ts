import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export type UmbPropertyEditorUIPickerModalData = {
	/**
	 * Whether to offer the Property Editor UIs that are deprecated.
	 * @default false
	 */
	showDeprecated?: boolean;
};

export type UmbPropertyEditorUIPickerModalValue = {
	selection: Array<string>;
};

export const UMB_PROPERTY_EDITOR_UI_PICKER_MODAL = new UmbModalToken<
	UmbPropertyEditorUIPickerModalData,
	UmbPropertyEditorUIPickerModalValue
>('Umb.Modal.PropertyEditorUiPicker', {
	modal: {
		type: 'sidebar',
		size: 'medium',
	},
});
