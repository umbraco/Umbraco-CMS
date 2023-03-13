import { UmbModalToken } from '@umbraco-cms/modal';

export interface UmbCreateDocumentModalData {
	unique: string | null;
}

export interface UmbCreateDocumentModalResultData {
	documentType: string;
}

export const UMB_CREATE_DOCUMENT_MODAL_TOKEN = new UmbModalToken<
	UmbCreateDocumentModalData,
	UmbCreateDocumentModalResultData
>('Umb.Modal.CreateDocument', {
	type: 'sidebar',
	size: 'small',
});
