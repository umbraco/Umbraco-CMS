import { UmbModalToken } from 'src/packages/core/modal';

export interface UmbDictionaryItemPickerModalData {
	multiple: boolean;
	selection: string[];
}

export interface UmbDictionaryItemPickerModalResult {
	selection: Array<string | null>;
}

export const UMB_DICTIONARY_ITEM_PICKER_MODAL_ALIAS = 'Umb.Modal.DictionaryItemPicker';

export const UMB_DICTIONARY_ITEM_PICKER_MODAL = new UmbModalToken<
	UmbDictionaryItemPickerModalData,
	UmbDictionaryItemPickerModalResult
>(UMB_DICTIONARY_ITEM_PICKER_MODAL_ALIAS, {
	type: 'sidebar',
	size: 'small',
});
