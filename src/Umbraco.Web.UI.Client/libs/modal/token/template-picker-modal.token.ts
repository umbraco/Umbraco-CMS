import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTemplatePickerModalData {
	multiple: boolean;
	selection: Array<string | null>;
}

export interface UmbTemplatePickerModalResult {
	selection: Array<string | null>;
}

export const UMB_TEMPLATE_PICKER_MODAL = new UmbModalToken<UmbTemplatePickerModalData, UmbTemplatePickerModalResult>(
	'Umb.Modal.TemplatePicker',
	{
		type: 'sidebar',
		size: 'small',
	}
);
