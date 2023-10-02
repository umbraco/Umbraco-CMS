import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbTemplateModalData {
	id: string;
	language?: 'razor' | 'typescript' | 'javascript' | 'css' | 'markdown' | 'json' | 'html';
}

export interface UmbTemplateModalValue {
	id: string;
}

export const UMB_TEMPLATE_MODAL = new UmbModalToken<UmbTemplateModalData, UmbTemplateModalValue>('Umb.Modal.Template', {
	type: 'sidebar',
	size: 'full',
});
