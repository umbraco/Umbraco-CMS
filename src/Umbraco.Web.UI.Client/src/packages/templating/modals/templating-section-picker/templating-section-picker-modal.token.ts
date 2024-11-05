import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbTemplatingSectionPickerModalData {}

export type UmbTemplatingSectionPickerModalValue = {
	value: string;
};

export const UMB_TEMPLATING_SECTION_PICKER_MODAL = new UmbModalToken<
	UmbTemplatingSectionPickerModalData,
	UmbTemplatingSectionPickerModalValue
>('Umb.Modal.TemplatingSectionPicker', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
