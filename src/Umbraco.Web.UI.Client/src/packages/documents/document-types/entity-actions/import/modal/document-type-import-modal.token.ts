import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentTypeImportModalData {
	unique: string | null;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface UmbDocumentTypeImportModalValue {}

export const UMB_DOCUMENT_TYPE_IMPORT_MODAL = new UmbModalToken<
	UmbDocumentTypeImportModalData,
	UmbDocumentTypeImportModalValue
>('Umb.Modal.DocumentType.Import', {
	modal: {
		type: 'sidebar',
		size: 'small',
	},
});
