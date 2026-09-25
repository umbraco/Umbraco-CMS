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
	 * @param unique The unique identifier of the entity to process.
	 * @param abortSignal Aborted if the user cancels while this entity's request is in flight.
	 */
	process: (unique: string, abortSignal: AbortSignal) => Promise<{ error?: unknown }>;
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
	 * The operation to await, or a factory that starts it given an `AbortSignal`. The dialog is only shown if it
	 * does not settle within `delayMs`.
	 *
	 * Passing a factory also opts into a Cancel button once the dialog appears: clicking it aborts the signal, and
	 * the operation is expected to observe the signal and settle in response. Pass a plain Promise instead when the
	 * operation cannot be interrupted - there is no signal to give it, so no Cancel button is shown.
	 */
	operation: Promise<T> | ((abortSignal: AbortSignal) => Promise<T>);
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
	 * aborts the item currently being processed (if it observes the given signal) and stops before
	 * the next one.
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

		// Rejects when the user clicks Cancel (or escape/backdrop): aborts whichever item is currently in
		// flight, so a slow item (e.g. one with many descendants) doesn't have to finish before stopping.
		const abortController = new AbortController();
		modal.onSubmit().catch(() => abortController.abort());

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

				const { error } = await args.process(unique, abortController.signal);
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
	 * within `delayMs` (default 400ms). The dialog is closed once the operation settles. Pass `operation`
	 * as a factory to also offer a Cancel button once the dialog appears.
	 * @template T The type the awaited operation resolves to.
	 * @param {UmbEntityBulkActionIndeterminateArgs<T>} args - The dialog headline, the operation to await and the optional delay.
	 * @returns {Promise<T>} The resolved value of the awaited operation.
	 */
	async runIndeterminate<T>(args: UmbEntityBulkActionIndeterminateArgs<T>): Promise<T> {
		const delayMs = args.delayMs ?? 400;
		const cancellable = typeof args.operation === 'function';

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		if (!modalManager) throw new Error('Modal manager context not found');

		const abortController = new AbortController();

		// Started eagerly, before the timer is armed: the delayMs contract only shows the dialog if the work
		// hasn't settled by then, so the operation must already be running when the clock starts - deferring the
		// factory call would silently change that contract for cancellable callers.
		const operation = typeof args.operation === 'function' ? args.operation(abortController.signal) : args.operation;

		let modal: UmbModalContext<UmbEntityBulkActionProgressModalData, UmbEntityBulkActionProgressModalValue> | undefined;
		const timer = setTimeout(() => {
			modal = modalManager.open(this, UMB_ENTITY_BULK_ACTION_PROGRESS_MODAL, {
				data: { headline: args.headline, mode: 'indeterminate', cancellable },
				value: { total: 0, completed: 0 },
			});

			if (cancellable) {
				// Rejects when the user clicks Cancel (or escape/backdrop). We only abort - we do not await this,
				// as the modal is torn down independently in the `finally` below once `operation` settles.
				modal.onSubmit().catch(() => abortController.abort());
			}
		}, delayMs);

		try {
			return await operation;
		} finally {
			clearTimeout(timer);
			if (modal && !modal.isResolved()) {
				modal.submit();
			}
		}
	}
}
