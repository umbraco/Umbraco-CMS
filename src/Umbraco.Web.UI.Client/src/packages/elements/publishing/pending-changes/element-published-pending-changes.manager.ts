import type { UmbElementVariantModel } from '../../types.js';
import type {
	UmbElementPublishedPendingChangesManagerProcessArgs,
	UmbPublishedElementVariantWithPendingChanges,
} from './types.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbMergeContentVariantDataController } from '@umbraco-cms/backoffice/content';
import { jsonStringComparison, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

/**
 * Manages the pending changes for a published element.
 * @exports
 * @class UmbElementPublishedPendingChangesManager
 * @augments {UmbControllerBase}
 */
export class UmbElementPublishedPendingChangesManager extends UmbControllerBase {
	#variantsWithChanges = new UmbArrayState<UmbPublishedElementVariantWithPendingChanges>([], (x) =>
		x.variantId.toString(),
	);
	public readonly variantsWithChanges = this.#variantsWithChanges.asObservable();

	/**
	 * Checks each variant if there are any pending changes to publish.
	 * @param {UmbElementPublishedPendingChangesManagerProcessArgs} args - The arguments for the process.
	 * @param {UmbElementDetailModel} args.persistedData - The persisted element data.
	 * @param {UmbElementDetailModel} args.publishedData - The published element data.
	 * @returns {Promise<void>}
	 * @memberof UmbElementPublishedPendingChangesManager
	 */
	async process(args: UmbElementPublishedPendingChangesManagerProcessArgs): Promise<void> {
		if (!args.persistedData) throw new Error('Persisted data is missing');
		if (!args.publishedData) throw new Error('Published data is missing');
		if (args.persistedData.unique !== args.publishedData.unique)
			throw new Error('Persisted and published data does not have the same unique');

		const variantIds = args.persistedData.variants?.map((x) => UmbVariantId.Create(x)) ?? [];

		const pendingChangesPromises = variantIds.map(async (variantId) => {
			const mergedData = await new UmbMergeContentVariantDataController(this).process(
				args.publishedData,
				args.persistedData,
				[variantId],
				[variantId],
			);

			const mergedDataClone = structuredClone(mergedData);
			const publishedDataClone = structuredClone(args.publishedData);

			// remove dates from the comparison
			mergedDataClone.variants.forEach((variant) => this.#cleanVariantForComparison(variant));
			publishedDataClone.variants.forEach((variant) => this.#cleanVariantForComparison(variant));

			const hasChanges = jsonStringComparison(mergedDataClone, publishedDataClone) === false;

			if (hasChanges) {
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
	 * @returns {Array<UmbPublishedElementVariantWithPendingChanges>}
	 * @memberof UmbElementPublishedPendingChangesManager
	 */
	getVariantsWithChanges(): Array<UmbPublishedElementVariantWithPendingChanges> {
		return this.#variantsWithChanges.getValue();
	}

	#cleanVariantForComparison = (variant: UmbElementVariantModel) => {
		// The server seems to have some date mismatches when quickly
		// fetching an element after a save and comparing it to the published version.
		// This is a temporary workaround to not include these dates in the comparison.
		// eslint-disable-next-line @typescript-eslint/ban-ts-comment
		// @ts-expect-error
		delete variant.updateDate;
	};

	/**
	 * Clear all states/values.
	 */
	clear() {
		this.#variantsWithChanges.setValue([]);
	}
}
