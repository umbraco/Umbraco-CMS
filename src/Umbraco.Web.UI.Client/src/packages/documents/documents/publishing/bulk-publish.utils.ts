import type { UmbDocumentVariantOptionModel } from '../types.js';
import type { UmbNotificationContext, UmbNotificationHandler } from '@umbraco-cms/backoffice/notification';
import type { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export type UmbBulkPublishResult = { succeeded: number; failed: number };

/**
 * Builds variant options from a list of languages.
 * Used by bulk publish/unpublish to create the variant picker options.
 */
export function buildVariantOptions(languages: Array<UmbLanguageDetailModel>): Array<UmbDocumentVariantOptionModel> {
	return languages.map((language) => ({
		language,
		variant: {
			name: language.name,
			culture: language.unique,
			state: null,
			createDate: null,
			publishDate: null,
			updateDate: null,
			segment: null,
			scheduledPublishDate: null,
			scheduledUnpublishDate: null,
			flags: [],
		},
		unique: new UmbVariantId(language.unique, null).toString(),
		culture: language.unique,
		segment: null,
	}));
}

/**
 * Shows a result notification for bulk operations.
 * @param type - 'publish' or 'unpublish' to determine localization keys
 */
export function showBulkResultNotification(
	notificationContext: UmbNotificationContext | undefined,
	localize: UmbLocalizationController,
	type: 'publish' | 'unpublish',
	succeeded: number,
	failed: number,
	total: number,
	variantIds: Array<UmbVariantId>,
) {
	const headline =
		type === 'publish'
			? localize.term('speechBubbles_editContentPublishedHeader')
			: localize.term('speechBubbles_contentUnpublished');

	if (failed > 0) {
		const messageKey =
			type === 'publish'
				? 'speechBubbles_editMultiContentPublishedPartialText'
				: 'speechBubbles_editMultiContentUnpublishedPartialText';

		notificationContext?.peek('warning', {
			data: {
				headline,
				message: localize.term(messageKey, succeeded, total, failed),
			},
		});
	} else {
		let message: string;
		if (variantIds.length === 1) {
			message = localize.term(
				type === 'publish'
					? 'speechBubbles_editMultiContentPublishedText'
					: 'speechBubbles_editMultiContentUnpublishedText',
				succeeded,
			);
		} else {
			message = localize.term(
				type === 'publish'
					? 'speechBubbles_editMultiVariantPublishedText'
					: 'speechBubbles_editMultiVariantUnpublishedText',
				succeeded,
				localize.list(variantIds.map((v) => v.culture ?? '')),
			);
		}

		notificationContext?.peek('positive', {
			data: { headline, message },
		});
	}
}

/**
 * Processes documents sequentially with live progress updates.
 * @param {Array<string>} documentUniques - Array of document unique identifiers to process
 * @param {function(string): Promise<{error?: unknown}>} processFn - Function to call for each document
 * @param {UmbNotificationContext | undefined} notificationContext - Notification context for showing progress
 * @param {string} progressHeadline - Headline to show in the progress notification
 * @returns {Promise<UmbBulkPublishResult>} Object with succeeded and failed counts
 */
export async function processDocumentsInBatches(
	documentUniques: Array<string>,
	processFn: (unique: string) => Promise<{ error?: unknown }>,
	notificationContext: UmbNotificationContext | undefined,
	progressHeadline: string,
): Promise<UmbBulkPublishResult> {
	const total = documentUniques.length;
	let processed = 0;
	let succeeded = 0;
	let failed = 0;

	// Show progress notification that stays until we close it
	let progressNotice: UmbNotificationHandler | undefined;

	const updateProgress = () => {
		const message = `${processed} / ${total}`;

		if (progressNotice) {
			// Update existing notification data
			progressNotice.updateData({ headline: progressHeadline, message });
		} else {
			// Create initial notification
			progressNotice = notificationContext?.stay('warning', {
				data: { headline: progressHeadline, message },
			});
		}
	};

	// Show initial progress
	updateProgress();

	try {
		// Process documents sequentially (SQLite can't handle concurrent writes)
		for (const unique of documentUniques) {
			const { error } = await processFn(unique);

			processed++;
			if (!error) {
				succeeded++;
			} else {
				failed++;
			}

			// Update progress after each document
			updateProgress();
		}
	} finally {
		// Always close progress notification
		progressNotice?.close();
	}

	return { succeeded, failed };
}
