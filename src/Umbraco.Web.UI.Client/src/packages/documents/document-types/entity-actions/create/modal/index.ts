import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentTypeCreateOptionsModalData {
	parentKey: string | null;
}

export const UMB_DOCUMENT_TYPE_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbDocumentTypeCreateOptionsModalData>(
	'Umb.Modal.DocumentTypeCreateOptions',
	{
		config: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
