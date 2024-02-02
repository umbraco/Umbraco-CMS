import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentCreateOptionsModalData {
	document: {
		unique: string;
	} | null;
	documentType: {
		unique: string;
	} | null;
}

export interface UmbDocumentCreateOptionsModalValue {}

export const UMB_DOCUMENT_CREATE_OPTIONS_MODAL = new UmbModalToken<
	UmbDocumentCreateOptionsModalData,
	UmbDocumentCreateOptionsModalValue
>('Umb.Modal.Document.CreateOptions', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
