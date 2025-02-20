import type { UmbClipboardEntryDetailModel } from '../types.js';
import { UMB_CLIPBOARD_ENTRY_PICKER_MODAL_ALIAS } from './constants.js';
import { UmbModalToken, type UmbPickerModalData, type UmbPickerModalValue } from '@umbraco-cms/backoffice/modal';

export interface UmbClipboardEntryPickerModalData extends UmbPickerModalData<UmbClipboardEntryDetailModel> {
	asyncFilter?: (item: UmbClipboardEntryDetailModel) => Promise<boolean>;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbClipboardEntryPickerModalValue extends UmbPickerModalValue {}

export const UMB_CLIPBOARD_ENTRY_PICKER_MODAL = new UmbModalToken<
	UmbClipboardEntryPickerModalData,
	UmbClipboardEntryPickerModalValue
>(UMB_CLIPBOARD_ENTRY_PICKER_MODAL_ALIAS, {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
