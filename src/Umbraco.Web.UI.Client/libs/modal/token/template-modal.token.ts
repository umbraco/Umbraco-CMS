import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTemplateModalData {
	key: string;
	language?: 'razor' | 'typescript' | 'javascript' | 'css' | 'markdown' | 'json' | 'html';
}

export interface UmbTemplateModalResult {
	key: string;
}

export const UMB_TEMPLATE_MODAL = new UmbModalToken<UmbTemplateModalData, UmbTemplateModalResult>(
	'Umb.Modal.Template',
	{
		type: 'sidebar',
		size: 'full',
	}
);
