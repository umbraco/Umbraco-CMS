import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// TODO: Stop sending the initial selection as part of data [NL], it should just be in the value:
export interface UmbCompositionPickerModalData {
	compositionRepositoryAlias: string;
	selection: Array<string>;
	unique: string | null;
	isElement: boolean;
	currentPropertyAliases: Array<string>;
	isNew: boolean;
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
