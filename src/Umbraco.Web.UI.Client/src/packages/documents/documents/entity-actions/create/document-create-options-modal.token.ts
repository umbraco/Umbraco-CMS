import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentCreateOptionsModalData {
	parent: UmbEntityModel;
	documentType: {
		unique: string;
	} | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
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
