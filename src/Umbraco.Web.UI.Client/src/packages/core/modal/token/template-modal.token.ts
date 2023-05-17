import { UmbModalToken } from 'src/packages/core/modal';

export interface UmbTemplateModalData {
	id: string;
	language?: 'razor' | 'typescript' | 'javascript' | 'css' | 'markdown' | 'json' | 'html';
}

export interface UmbTemplateModalResult {
	id: string;
}

export const UMB_TEMPLATE_MODAL = new UmbModalToken<UmbTemplateModalData, UmbTemplateModalResult>(
	'Umb.Modal.Template',
	{
		type: 'sidebar',
		size: 'full',
	}
);
