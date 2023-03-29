import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTemplatePickerModalData {
	multiple: boolean;
	selection: string[];
}

export interface UmbTemplatePickerModalResult {
	selection: string[] | undefined;
}

// TODO: Move to libs:
export const UMB_TEMPLATE_PICKER_MODAL_TOKEN = new UmbModalToken<
	UmbTemplatePickerModalData,
	UmbTemplatePickerModalResult
>('Umb.Modal.TemplatePicker', {
	type: 'sidebar',
	size: 'small',
});
