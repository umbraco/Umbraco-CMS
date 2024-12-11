import type {
	UmbDocumentPublishedPendingChangesManagerProcessArgs,
	UmbPublishedVariantWithPendingChanges,
} from './types.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbMergeContentVariantDataController } from '@umbraco-cms/backoffice/content';
import { jsonStringComparison, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Manages the pending changes for a published document.
 * @exports
 * @class UmbDocumentPublishedPendingChangesManager
 * @augments {UmbControllerBase}
 */
export class UmbDocumentPublishedPendingChangesManager extends UmbControllerBase {
	#variantsWithChanges = new UmbArrayState<UmbPublishedVariantWithPendingChanges>([], (x) => x.variantId.toString());
	public readonly variantsWithChanges = this.#variantsWithChanges.asObservable();

	/**
	 * Checks each variant if there are any pending changes to publish.
	 * @param {UmbDocumentPublishedPendingChangesManagerProcessArgs} args - The arguments for the process.
	 * @param {UmbDocumentDetailModel} args.persistedData - The persisted document data.
	 * @param {UmbDocumentDetailModel} args.publishedData - The published document data.
	 * @returns {Promise<void>}
	 * @memberof UmbDocumentPublishedPendingChangesManager
	 */
	async process(args: UmbDocumentPublishedPendingChangesManagerProcessArgs): Promise<void> {
		if (!args.persistedData) throw new Error('Persisted data is missing');
		if (!args.publishedData) throw new Error('Published data is missing');

		const variantIds = args.persistedData.variants?.map((x) => UmbVariantId.Create(x)) ?? [];

		const pendingChangesPromises = variantIds.map(async (variantId) => {
			const mergedData = await new UmbMergeContentVariantDataController(this).process(
				args.publishedData,
				args.persistedData,
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

	/**
	 * Gets the variants with changes.
	 * @returns {Array<UmbPublishedVariantWithPendingChanges>}  {Array<UmbVariantWithChanges>}
	 * @memberof UmbDocumentPublishedPendingChangesManager
	 */
	getVariantsWithChanges(): Array<UmbPublishedVariantWithPendingChanges> {
		return this.#variantsWithChanges.getValue();
	}
}
