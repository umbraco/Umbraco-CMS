import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCompositionPickerModalData {
	unique: string;
	selection: Array<string>;
}

export interface UmbCompositionPickerModalValue {
	selection: Array<string>;
}

export const UMB_COMPOSITION_PICKER_MODAL = new UmbModalToken<
	UmbCompositionPickerModalData,
	UmbCompositionPickerModalValue
>('Umb.Modal.CompositionPicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
