import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbPropertyEditorUIPickerModalData {
	selection?: Array<string>;
	submitLabel?: string;
}

export const UMB_PROPERTY_EDITOR_UI_PICKER_MODAL_TOKEN = new UmbModalToken<UmbPropertyEditorUIPickerModalData>(
	'Umb.Modal.PropertyEditorUIPicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
