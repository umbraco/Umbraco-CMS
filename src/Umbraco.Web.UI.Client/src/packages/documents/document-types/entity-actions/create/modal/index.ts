import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentTypeCreateOptionsModalData {
	parent: UmbEntityModel;
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
