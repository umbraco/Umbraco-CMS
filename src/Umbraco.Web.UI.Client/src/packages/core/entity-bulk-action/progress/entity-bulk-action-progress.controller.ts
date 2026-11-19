import { UMB_ENTITY_BULK_ACTION_PROGRESS_MODAL } from './entity-bulk-action-progress-modal.token.js';
import type {
	UmbEntityBulkActionProgressModalData,
	UmbEntityBulkActionProgressModalValue,
} from './entity-bulk-action-progress-modal.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';

export interface UmbEntityBulkActionProgressArgs {
	/**
	 * The headline shown in the progress dialog.
	 */
	headline: string;
	/**
	 * The unique identifiers of the entities to process.
	 */
	uniques: Array<string>;
	/**
	 * Called once per entity, sequentially. Sequential processing is intentional:
	 * concurrent writes cause database locking (notably with SQLite).
	 */
	process: (unique: string) => Promise<{ error?: unknown }>;
}

export interface UmbEntityBulkActionProgressResult {
	/**
	 * The number of entities that were processed successfully.
	 */
	succeeded: number;
	/**
	 * The number of entities that failed to process.
	 */
	failed: number;
	/**
	 * Whether the operation was stopped by the user closing the dialog before completion.
	 */
	cancelled: boolean;
}

export interface UmbEntityBulkActionIndeterminateArgs<T> {
	/**
	 * The headline shown in the progress dialog.
	 */
	headline: string;
	/**
	 * The operation to await. The dialog is only shown if it does not settle within `delayMs`.
	 */
	operation: Promise<T>;
	/**
	 * How long to wait before showing the dialog. Defaults to 400ms.
	 */
	delayMs?: number;
}

/**
 * Presents progress for a bulk entity action while it runs.
 *
 * Use `runWithProgress` for a sequence of per-item operations with a determinate counter and a
 * cancel affordance, or `runIndeterminate` for a single opaque operation that only surfaces a
 * spinner if it takes longer than a short delay.
 */
export class UmbEntityBulkActionProgressController extends UmbControllerBase {
	/**
	 * Runs a bulk operation sequentially while presenting a determinate progress dialog with a
	 * "X / Y" counter and a cancel affordance. Closing the dialog (cancel button, escape or backdrop)
	 * stops the operation after the item currently being processed.
	 * @param {UmbEntityBulkActionProgressArgs} args - The dialog headline, the uniques to process and the per-item processor.
	 * @returns {Promise<UmbEntityBulkActionProgressResult>} The succeeded/failed counts and whether the user cancelled.
	 */
	async runWithProgress(args: UmbEntityBulkActionProgressArgs): Promise<UmbEntityBulkActionProgressResult> {
		const total = args.uniques.length;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) throw new Error('Modal manager context not found');

		const modal = modalManager.open(this, UMB_ENTITY_BULK_ACTION_PROGRESS_MODAL, {
			data: { headline: args.headline, mode: 'determinate' },
			value: { total, completed: 0 },
		});

		let succeeded = 0;
		let failed = 0;
		let completed = 0;
		let cancelled = false;

		try {
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
		} finally {
			// Close the dialog now that the operation has finished — including if `process` threw,
			// so a contract violation can never leave the dialog blocking the UI.
			if (!modal.isResolved()) {
				modal.submit();
			}
		}

		return { succeeded, failed, cancelled };
	}

	/**
	 * Awaits a single operation, showing an indeterminate progress dialog only if it does not settle
	 * within `delayMs` (default 400ms). There is no cancel affordance. The dialog is closed once the
	 * operation settles.
	 * @template T The type the awaited operation resolves to.
	 * @param {UmbEntityBulkActionIndeterminateArgs<T>} args - The dialog headline, the operation to await and the optional delay.
	 * @returns {Promise<T>} The resolved value of the awaited operation.
	 */
	async runIndeterminate<T>(args: UmbEntityBulkActionIndeterminateArgs<T>): Promise<T> {
		const delayMs = args.delayMs ?? 400;

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) throw new Error('Modal manager context not found');

		let modal: UmbModalContext<UmbEntityBulkActionProgressModalData, UmbEntityBulkActionProgressModalValue> | undefined;
		const timer = setTimeout(() => {
			modal = modalManager.open(this, UMB_ENTITY_BULK_ACTION_PROGRESS_MODAL, {
				data: { headline: args.headline, mode: 'indeterminate' },
				value: { total: 0, completed: 0 },
			});
		}, delayMs);

		try {
			return await args.operation;
		} finally {
			clearTimeout(timer);
			if (modal && !modal.isResolved()) {
				modal.submit();
			}
		}
	}
}
