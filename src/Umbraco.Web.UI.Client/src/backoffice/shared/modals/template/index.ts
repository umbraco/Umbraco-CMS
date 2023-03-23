import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTemplateModalData {
	multiple: boolean;
	selection: string[];
}

export interface UmbTemplateModalResult {
	selection: string[] | undefined;
}

export const UMB_TEMPLATE_MODAL_TOKEN = new UmbModalToken<UmbTemplateModalData, UmbTemplateModalResult>(
	'Umb.Modal.Template',
	{
		type: 'sidebar',
		size: 'large',
	}
);
