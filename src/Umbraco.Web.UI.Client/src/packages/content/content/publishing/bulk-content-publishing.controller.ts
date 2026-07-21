import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbEntityBulkActionProgressController } from '@umbraco-cms/backoffice/entity-bulk-action';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * The localization keys used to phrase the single result notification for a bulk publish/unpublish.
 * Each content type (documents, elements, …) supplies its own keys so the wording matches the entity.
 */
export interface UmbBulkContentPublishingResultLabels {
	/**
	 * Notification headline.
	 */
	headline: string;
	/**
	 * Success message when more than one variant was processed. Receives the succeeded count and the
	 * list of cultures.
	 */
	multiVariant: string;
	/**
	 * Success message when a single variant was processed. Receives the succeeded count.
	 */
	multiContent: string;
	/**
	 * Warning message when only some of the selection was processed. Receives the succeeded count and
	 * the total.
	 */
	partial: string;
}

export interface UmbBulkContentPublishingArgs {
	/**
	 * The content items to (un)publish.
	 */
	selection: Array<string>;
	/**
	 * The parent entity to reload once the operation succeeds.
	 */
	entityType: string;
	unique: string | null;
	/**
	 * Localization key for the progress dialog headline.
	 */
	headlineKey: string;
	/**
	 * The variants the operation is performed for, used to phrase the result notification.
	 */
	variantIds: Array<UmbVariantId>;
	/**
	 * Localization keys for the result notification.
	 */
	labels: UmbBulkContentPublishingResultLabels;
	/**
	 * Performs the operation for a single item. Called once per selected item, sequentially.
	 */
	process: (unique: string) => Promise<{ error?: unknown }>;
}

/**
 * Shared tail for content "publish" and "unpublish" bulk actions: run the per-item operation behind a
 * determinate progress dialog, report the outcome in a single notification, then reload the parent
 * entity when anything succeeded. Consumers (documents, elements, …) supply the per-item operation and
 * the localization keys; the orchestration is identical across content types.
 */
export class UmbBulkContentPublishingController extends UmbControllerBase {
	async run(args: UmbBulkContentPublishingArgs): Promise<void> {
		const localize = new UmbLocalizationController(this);

		const result = await new UmbEntityBulkActionProgressController(this).runWithProgress({
			headline: localize.term(args.headlineKey),
			uniques: args.selection,
			process: args.process,
		});

		await this.#notify(localize, args, result.succeeded);

		if (result.succeeded > 0) {
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			eventContext?.dispatchEvent(
				new UmbRequestReloadChildrenOfEntityEvent({ entityType: args.entityType, unique: args.unique }),
			);
		}
	}

	async #notify(
		localize: UmbLocalizationController,
		args: UmbBulkContentPublishingArgs,
		succeeded: number,
	): Promise<void> {
		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
		if (!notificationContext) return;

		const { labels, variantIds } = args;
		const total = args.selection.length;
		const headline = localize.term(labels.headline);

		if (succeeded === total) {
			const message =
				variantIds.length > 1
					? localize.term(labels.multiVariant, succeeded, localize.list(variantIds.map((v) => v.culture ?? '')))
					: localize.term(labels.multiContent, succeeded);
			notificationContext.peek('positive', { data: { headline, message } });
		} else {
			notificationContext.peek('warning', {
				data: { headline, message: localize.term(labels.partial, succeeded, total) },
			});
		}
	}
}
