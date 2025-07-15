import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export interface UmbDocumentTypeCreateOptionsModalData {
	parent: UmbEntityModel;
}

/** @deprecated No longer used internally. This will be removed in Umbraco 17. [LK] */
export const UMB_DOCUMENT_TYPE_CREATE_OPTIONS_MODAL = new UmbModalToken<UmbDocumentTypeCreateOptionsModalData>(
	'Umb.Modal.DocumentTypeCreateOptions',
	{
		modal: {
			type: 'sidebar',
			size: 'small',
		},
	},
);
