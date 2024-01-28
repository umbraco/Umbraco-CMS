import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbCreateDocumentModalData {
	document: {
		unique: string;
	} | null;
	documentType: {
		unique: string;
	} | null;
}

export interface UmbCreateDocumentModalValue {}

export const UMB_CREATE_DOCUMENT_MODAL = new UmbModalToken<UmbCreateDocumentModalData, UmbCreateDocumentModalValue>(
	'Umb.Modal.CreateDocument',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
