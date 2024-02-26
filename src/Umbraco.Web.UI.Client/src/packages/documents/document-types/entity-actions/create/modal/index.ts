import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentTypeCreateOptionsModalData {
	parentUnique: string | null;
	entityType: string;
}

export const UMB_DOCUMENT_TYPE_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbDocumentTypeCreateOptionsModalData>(
	'Umb.Modal.DocumentTypeCreateOptions',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
