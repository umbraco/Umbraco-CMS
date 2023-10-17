import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateDocumentModalData {
	id: string | null;
}

export interface UmbCreateDocumentModalValue {
	documentTypeId: string;
}

export const UMB_CREATE_DOCUMENT_MODAL = new UmbModalToken<UmbCreateDocumentModalData, UmbCreateDocumentModalValue>(
	'Umb.Modal.CreateDocument',
	{
		type: 'sidebar',
		size: 'small',
	},
);
