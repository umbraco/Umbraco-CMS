import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export interface UmbBulkPublishingResultArgs {
	/**
	 * The number of documents that were (un)published successfully.
	 */
	succeeded: number;
	/**
	 * The total number of documents in the selection.
	 */
	total: number;
	/**
	 * The variants the operation was performed for, used to phrase the message.
	 */
	variantIds: Array<UmbVariantId>;
}

/**
 * Shows the single result notification for a bulk publish/unpublish operation: a positive notification
 * when everything succeeded, or a warning with the partial count when some documents were not processed.
 * @param {UmbNotificationContext | undefined} notificationContext - The notification context to peek on.
 * @param {UmbLocalizationController} localize - The localization controller.
 * @param {'publish' | 'unpublish'} type - Which operation was performed, selecting the message keys.
 * @param {UmbBulkPublishingResultArgs} args - The succeeded/total counts and the variants involved.
 */
export function showBulkPublishingResultNotification(
	notificationContext: UmbNotificationContext | undefined,
	localize: UmbLocalizationController,
	type: 'publish' | 'unpublish',
	args: UmbBulkPublishingResultArgs,
): void {
	const { succeeded, total, variantIds } = args;
	const headline = localize.term(
		type === 'publish' ? 'speechBubbles_editContentPublishedHeader' : 'speechBubbles_contentUnpublished',
	);

	if (succeeded === total) {
		const message =
			variantIds.length > 1
				? localize.term(
						type === 'publish'
							? 'speechBubbles_editMultiVariantPublishedText'
							: 'speechBubbles_editMultiVariantUnpublishedText',
						succeeded,
						localize.list(variantIds.map((v) => v.culture ?? '')),
					)
				: localize.term(
						type === 'publish'
							? 'speechBubbles_editMultiContentPublishedText'
							: 'speechBubbles_editMultiContentUnpublishedText',
						succeeded,
					);
		notificationContext?.peek('positive', { data: { headline, message } });
	} else {
		notificationContext?.peek('warning', {
			data: {
				headline,
				message: localize.term(
					type === 'publish'
						? 'speechBubbles_editMultiContentPublishedPartialText'
						: 'speechBubbles_editMultiContentUnpublishedPartialText',
					succeeded,
					total,
				),
			},
		});
	}
}
