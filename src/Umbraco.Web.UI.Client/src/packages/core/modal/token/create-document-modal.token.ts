import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateDocumentModalData {
	id: string | null;
}

export interface UmbCreateDocumentModalResult {
	documentTypeId: string;
}

export const UMB_CREATE_DOCUMENT_MODAL = new UmbModalToken<UmbCreateDocumentModalData, UmbCreateDocumentModalResult>(
	'Umb.Modal.CreateDocument',
	{
		type: 'sidebar',
		size: 'small',
	},
);
