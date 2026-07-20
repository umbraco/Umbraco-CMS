import { UMB_DOCUMENT_BULK_PUBLISHING_PROGRESS_MODAL } from './bulk-publishing-progress-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

export interface UmbDocumentBulkPublishingProgressArgs {
	/**
	 * The headline shown in the progress dialog.
	 */
	headline: string;
	/**
	 * The unique identifiers of the documents to process.
	 */
	uniques: Array<string>;
	/**
	 * Called once per document, sequentially. Sequential processing is intentional:
	 * concurrent publish writes cause database locking (notably with SQLite).
	 */
	process: (unique: string) => Promise<{ error?: unknown }>;
}

export interface UmbDocumentBulkPublishingProgressResult {
	/**
	 * The number of documents that were processed successfully.
	 */
	succeeded: number;
	/**
	 * The number of documents that failed to process.
	 */
	failed: number;
	/**
	 * Whether the operation was stopped by the user closing the dialog before completion.
	 */
	cancelled: boolean;
}

/**
 * Runs a bulk publishing operation sequentially while presenting a progress dialog.
 * The dialog makes it clear that the user must keep it open for the process to continue;
 * closing it (via the cancel button, the escape key or the backdrop) stops the operation
 * after the document currently being processed.
 */
export class UmbDocumentBulkPublishingProgressController extends UmbControllerBase {
	async run(args: UmbDocumentBulkPublishingProgressArgs): Promise<UmbDocumentBulkPublishingProgressResult> {
		const total = args.uniques.length;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) throw new Error('Modal manager context not found');

		const modal = modalManager.open(this, UMB_DOCUMENT_BULK_PUBLISHING_PROGRESS_MODAL, {
			data: { headline: args.headline },
			value: { total, completed: 0 },
		});

		let succeeded = 0;
		let failed = 0;
		let completed = 0;
		let cancelled = false;

		for (const unique of args.uniques) {
			// Closing the dialog (cancel, escape, backdrop or navigation) resolves the modal — stop here.
			// We must not touch the modal once resolved, as its state is torn down.
			if (modal.isResolved()) {
				cancelled = true;
				break;
			}

			const { error } = await args.process(unique);
			completed++;
			if (error) {
				failed++;
			} else {
				succeeded++;
			}

			if (modal.isResolved()) {
				cancelled = true;
				break;
			}

			modal.setValue({ total, completed });
		}

		// If the dialog is still open, close it now that the operation has finished.
		if (!modal.isResolved()) {
			modal.submit();
		}

		return { succeeded, failed, cancelled };
	}
}
