import type { UmbContentDetailModel } from '../types.js';
import type { UmbPublishedVariantWithPendingChanges } from './types.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbMergeContentVariantDataController } from '../controller/merge-content-variant-data.controller.js';
import { jsonStringComparison, UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export interface UmbPublishedPendingChangesManagerProcessArgs<TDetailModel extends UmbContentDetailModel> {
	persistedData: TDetailModel;
	publishedData: TDetailModel;
}

/**
 * Base class for managing pending changes between persisted and published content.
 * @exports
 * @abstract
 * @class UmbPublishedPendingChangesManagerBase
 * @augments {UmbControllerBase}
 */
export abstract class UmbPublishedPendingChangesManagerBase<
	TDetailModel extends UmbContentDetailModel<TVariantModel>,
	TVariantModel extends UmbEntityVariantModel = UmbEntityVariantModel,
> extends UmbControllerBase {
	#variantsWithChanges = new UmbArrayState<UmbPublishedVariantWithPendingChanges>([], (x) => x.variantId.toString());
	/** Observable emitting the list of variants that have unpublished changes. */
	public readonly variantsWithChanges = this.#variantsWithChanges.asObservable();

	/**
	 * Checks each variant if there are any pending changes to publish.
	 * @param {UmbPublishedPendingChangesManagerProcessArgs<TDetailModel>} args - The arguments for the process.
	 * @returns {Promise<void>}
	 */
	async process(args: UmbPublishedPendingChangesManagerProcessArgs<TDetailModel>): Promise<void> {
		if (!args.persistedData) throw new Error('Persisted data is missing');
		if (!args.publishedData) throw new Error('Published data is missing');
		if (args.persistedData.unique !== args.publishedData.unique)
			throw new Error('Persisted and published data does not have the same unique');

		const variantIds = args.persistedData.variants?.map((x) => UmbVariantId.Create(x)) ?? [];
		const pendingChangesPromises = variantIds.map((variantId) => this.#processVariant(args, variantId));
		const variantsWithPendingChanges = (await Promise.all(pendingChangesPromises)).filter((x) => x !== null);
		this.#variantsWithChanges.setValue(variantsWithPendingChanges);
	}

	async #processVariant(
		args: UmbPublishedPendingChangesManagerProcessArgs<TDetailModel>,
		variantId: UmbVariantId,
	): Promise<UmbPublishedVariantWithPendingChanges | null> {
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

		this.cleanDataBeforeComparison(mergedDataClone, publishedDataClone);

		const hasChanges = jsonStringComparison(mergedDataClone, publishedDataClone) === false;
		return hasChanges ? { variantId } : null;
	}

	/**
	 * Gets the variants with changes.
	 * @returns {Array<UmbPublishedVariantWithPendingChanges>}
	 */
	getVariantsWithChanges(): Array<UmbPublishedVariantWithPendingChanges> {
		return this.#variantsWithChanges.getValue();
	}

	/**
	 * Hook to clean data before comparison. Override to remove properties
	 * that should not affect the pending-changes calculation.
	 * @param {TDetailModel} _mergedData - The merged data clone.
	 * @param {TDetailModel} _publishedData - The published data clone.
	 */
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	protected cleanDataBeforeComparison(_mergedData: TDetailModel, _publishedData: TDetailModel): void {
		// No-op by default. Subclasses can override.
	}

	#cleanVariantForComparison = (variant: TVariantModel) => {
		// The server seems to have some date mismatches when quickly
		// fetching content after a save and comparing it to the published version.
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
