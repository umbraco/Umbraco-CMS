import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

export const UMB_ENTITY_BULK_ACTION_PROGRESS_MODAL_ALIAS = 'Umb.Modal.EntityBulkActionProgress';

export interface UmbEntityBulkActionProgressModalData {
	/**
	 * The headline shown in the progress dialog.
	 */
	headline: string;
	/**
	 * Whether the dialog shows a determinate counter (with cancel) or an indeterminate spinner.
	 */
	mode: 'determinate' | 'indeterminate';
	/**
	 * Shows a Cancel button in indeterminate mode. Ignored in determinate mode, which always has one.
	 */
	cancellable?: boolean;
}

export interface UmbEntityBulkActionProgressModalValue {
	/**
	 * The total number of items to process.
	 */
	total: number;
	/**
	 * The number of items processed so far.
	 */
	completed: number;
}

export const UMB_ENTITY_BULK_ACTION_PROGRESS_MODAL = new UmbModalToken<
	UmbEntityBulkActionProgressModalData,
	UmbEntityBulkActionProgressModalValue
>(UMB_ENTITY_BULK_ACTION_PROGRESS_MODAL_ALIAS, {
	modal: {
		type: 'dialog',
	},
});
