import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_DOCUMENT_BULK_PUBLISHING_PROGRESS_MODAL_ALIAS = 'Umb.Modal.Document.BulkPublishingProgress';

export interface UmbDocumentBulkPublishingProgressModalData {
	/**
	 * The headline shown in the progress dialog.
	 */
	headline: string;
}

export interface UmbDocumentBulkPublishingProgressModalValue {
	/**
	 * The total number of documents to process.
	 */
	total: number;
	/**
	 * The number of documents processed so far.
	 */
	completed: number;
}

export const UMB_DOCUMENT_BULK_PUBLISHING_PROGRESS_MODAL = new UmbModalToken<
	UmbDocumentBulkPublishingProgressModalData,
	UmbDocumentBulkPublishingProgressModalValue
>(UMB_DOCUMENT_BULK_PUBLISHING_PROGRESS_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
