import { UmbModalToken } from 'src/packages/core/modal';

export interface UmbPartialViewPickerModalData {
	multiple: boolean;
	selection: string[];
}

export interface UmbPartialViewPickerModalResult {
	selection: Array<string | null> | undefined;
}

export const UMB_PARTIAL_VIEW_PICKER_MODAL_ALIAS = 'Umb.Modal.PartialViewPicker';

export const UMB_PARTIAL_VIEW_PICKER_MODAL = new UmbModalToken<
	UmbPartialViewPickerModalData,
	UmbPartialViewPickerModalResult
>(UMB_PARTIAL_VIEW_PICKER_MODAL_ALIAS, {
	type: 'sidebar',
	size: 'small',
});
