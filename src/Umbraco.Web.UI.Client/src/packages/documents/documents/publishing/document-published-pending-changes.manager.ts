import type { UmbDocumentDetailModel } from '../types.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbMergeContentVariantDataController } from '@umbraco-cms/backoffice/content';
import { jsonStringComparison, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

interface UmbDocumentPublishedPendingChangesManagerProcessArgs {
	currentData: UmbDocumentDetailModel;
	publishedData: UmbDocumentDetailModel;
}

interface UmbVariantWithChanges {
	variantId: UmbVariantId;
}

export class UmbDocumentPublishedPendingChangesManager extends UmbControllerBase {
	#variantsWithChanges = new UmbArrayState<UmbVariantWithChanges>([], (x) => x.variantId.toString());
	public readonly variantsWithChanges = this.#variantsWithChanges.asObservable();

	/**
	 * Checks each variant if there are any pending changes to publish.
	 * @param {UmbDocumentPublishedPendingChangesManagerProcessArgs} args - The arguments for the process.
	 * @param {string} args.unique - The unique identifier of the document.
	 * @param {UmbDocumentDetailModel} args.currentData - The current document data.
	 * @returns {Promise<void>}
	 * @memberof UmbPublishedPendingChangesManager
	 */
	async process(args: UmbDocumentPublishedPendingChangesManagerProcessArgs): Promise<void> {
		if (!args.currentData) throw new Error('Current Data is missing');
		if (!args.publishedData) throw new Error('Published Data is missing');

		const variantIds = args.currentData.variants?.map((x) => UmbVariantId.Create(x)) ?? [];

		const pendingChangesPromises = variantIds.map(async (variantId) => {
			const mergedData = await new UmbMergeContentVariantDataController(this).process(
				args.publishedData,
				args.currentData,
				[variantId],
				[variantId],
			);

			if (jsonStringComparison(mergedData, args.publishedData) === false) {
				return { variantId };
			} else {
				return null;
			}
		});

		const variantsWithPendingChanges = (await Promise.all(pendingChangesPromises)).filter((x) => x !== null);

		this.#variantsWithChanges.setValue(variantsWithPendingChanges);
	}
}
