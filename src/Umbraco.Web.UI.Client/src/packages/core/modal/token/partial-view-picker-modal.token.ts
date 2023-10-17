import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbPartialViewPickerModalData {
	multiple: boolean;
	selection: string[];
}

export interface UmbPartialViewPickerModalValue {
	selection: Array<string | null> | undefined;
}

export const UMB_PARTIAL_VIEW_PICKER_MODAL_ALIAS = 'Umb.Modal.PartialViewPicker';

export const UMB_PARTIAL_VIEW_PICKER_MODAL = new UmbModalToken<
	UmbPartialViewPickerModalData,
	UmbPartialViewPickerModalValue
>(UMB_PARTIAL_VIEW_PICKER_MODAL_ALIAS, {
	type: 'sidebar',
	size: 'small',
});
