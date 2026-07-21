import { showBulkPublishingResultNotification } from './bulk-publishing-result-notification.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbEntityBulkActionProgressController } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbBulkDocumentPublishingArgs {
	/**
	 * The documents to (un)publish.
	 */
	selection: Array<string>;
	/**
	 * The parent entity to reload once the operation succeeds.
	 */
	entityType: string;
	unique: string | null;
	/**
	 * Which operation is being performed, selecting the result-notification message keys.
	 */
	type: 'publish' | 'unpublish';
	/**
	 * Localization term key for the progress dialog headline.
	 */
	headlineKey: string;
	/**
	 * The variants the operation is performed for, used to phrase the result notification.
	 */
	variantIds: Array<UmbVariantId>;
	/**
	 * Performs the operation for a single document. Called once per selected document, sequentially.
	 */
	process: (unique: string) => Promise<{ error?: unknown }>;
}

/**
 * Shared tail for the document "publish" and "unpublish" bulk actions: run the per-document operation
 * behind a determinate progress dialog, report the outcome in a single notification, then reload the
 * parent entity when anything succeeded.
 */
export class UmbBulkDocumentPublishingController extends UmbControllerBase {
	async run(args: UmbBulkDocumentPublishingArgs): Promise<void> {
		const localize = new UmbLocalizationController(this);

		const result = await new UmbEntityBulkActionProgressController(this).runWithProgress({
			headline: localize.term(args.headlineKey),
			uniques: args.selection,
			process: args.process,
		});

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		showBulkPublishingResultNotification(notificationContext, localize, args.type, {
			succeeded: result.succeeded,
			total: args.selection.length,
			variantIds: args.variantIds,
		});

		if (result.succeeded > 0) {
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			eventContext?.dispatchEvent(
				new UmbRequestReloadChildrenOfEntityEvent({ entityType: args.entityType, unique: args.unique }),
			);
		}
	}
}
