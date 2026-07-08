import type { UmbContentDetailModel } from '../types.js';
import { UmbElementWorkspaceDataManager } from './element-data-manager.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { appendToFrozenArray, jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId, type UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export class UmbContentWorkspaceDataManager<
	ModelType extends UmbContentDetailModel,
	ModelVariantType extends UmbEntityVariantModel = ModelType extends { variants: UmbEntityVariantModel[] }
		? ModelType['variants'][0]
		: never,
> extends UmbElementWorkspaceDataManager<ModelType> {
	//
	//#repository;
	#variantScaffold?: ModelVariantType;

	constructor(host: UmbControllerHost, variantScaffold?: ModelVariantType) {
		super(host);
		this.#variantScaffold = variantScaffold;
	}

	protected override _sortCurrentData<GivenType extends Partial<ModelType> = Partial<ModelType>>(
		persistedData: Partial<ModelType>,
		currentData: GivenType,
	): GivenType {
		currentData = super._sortCurrentData(persistedData, currentData);
		// Sort the variants in the same order as the persisted data:
		const persistedVariants = persistedData.variants;
		if (persistedVariants && currentData.variants) {
			return {
				...currentData,
				variants: [...currentData.variants].sort(function (a, b) {
					return (
						persistedVariants.findIndex((x) => x.culture === a.culture) -
						persistedVariants.findIndex((x) => x.culture === b.culture)
					);
				}),
			};
		}
		return currentData;
	}

	/**
	 * Sets the variant scaffold data
	 * @param {ModelVariantType} variantScaffold The variant scaffold data
	 * @memberof UmbContentWorkspaceDataManager
	 */
	setVariantScaffold(variantScaffold: ModelVariantType) {
		this.#variantScaffold = variantScaffold;
	}

	ensureVariantData(variantId: UmbVariantId) {
		this.updateVariantData(variantId);
	}

	ensureVariantsData(variantIds: UmbVariantId[]) {
		this.initiatePropertyValueChange();
		for (const variantId of variantIds) {
			this.updateVariantData(variantId);
		}
		this.finishPropertyValueChange();
	}

	updateVariantData(variantId: UmbVariantId, update?: Partial<ModelVariantType>) {
		if (!this.#variantScaffold) throw new Error('Variant scaffold data is missing');

		if (this._variesByCulture === true) {
			// If variant Id is invariant, we don't to have the variant appended to our data.
			if (variantId.isInvariant()) {
				return;
			}

			this.#updateVariantData(variantId, update);
			return;
		}

		if (this._variesBySegment === true) {
			// When varying by segment we need to handle the "unsegmented" variant as invariant.
			// The rest of the segmented variants will be handled as normal variant data.
			if (variantId.isInvariant()) {
				this.#updateInvariantData(update);
			} else {
				this.#updateVariantData(variantId, update);
			}
			return;
		}

		if (this._varies === false) {
			this.#updateInvariantData(update);
			return;
		}

		throw new Error('Varies by culture is missing');
	}

	#updateVariantData(variantId: UmbVariantId, update?: Partial<ModelVariantType>) {
		const currentData = this.getCurrent();
		if (!currentData) throw new Error('Data is missing');

		// If variant-id varies by segment:
		if (!variantId.isSegmentInvariant()) {
			throw new Error('A segmented variant cannot have variant data.');
		}

		const variant = currentData.variants.find((x) => variantId.culture === x.culture);
		const newVariants = appendToFrozenArray(
			currentData.variants,
			{
				...this.#variantScaffold,
				culture: variantId.culture,
				...variant,
				...update,
			} as ModelVariantType,
			(x) => variantId.culture === x.culture,
		) as Array<ModelVariantType>;
		this.updateCurrent({ variants: newVariants } as unknown as ModelType);
	}

	#updateInvariantData(update?: Partial<ModelVariantType>) {
		const currentData = this.getCurrent();
		if (!currentData) throw new Error('Data is missing');

		const variant = currentData.variants.find((x) => (x.culture = null));
		// Cause we are invariant, we will just overwrite all variants with this one:
		const newVariants = [
			{
				...this.#variantScaffold,
				culture: null,
				...variant,
				...update,
			} as ModelVariantType,
		];
		this.updateCurrent({ variants: newVariants } as unknown as ModelType);
	}

	getChangedVariants() {
		const persisted = this.getPersisted();
		const current = this.getCurrent();
		if (!current) throw new Error('Current data is missing');

		const changedVariants: Array<GatheredVariantData> = current?.variants.map((variant) => {
			const persistedVariant = persisted?.variants.find((x) => x.culture === variant.culture);
			return {
				culture: variant.culture,
				equal: persistedVariant ? jsonStringComparison(variant, persistedVariant) : false,
			};
		});

		const changedProperties: Array<GatheredVariantData> = current?.values.map((value) => {
			const variantId = UmbVariantId.Create(value);
			const persistedValues = persisted?.values.find((x) => x.alias === value.alias && variantId.compare(x));
			return {
				culture: value.culture,
				segment: value.segment,
				equal: persistedValues ? jsonStringComparison(value, persistedValues) : false,
			};
		});

		// calculate the variantIds of those who either have a change in properties or in variants:
		return (
			changedVariants
				?.concat(changedProperties ?? [])
				.filter((x) => x.equal === false)
				.map((x) => new UmbVariantId(x.culture, x.segment)) ?? []
		);
	}
}

type GatheredVariantData = {
	culture: string | null;
	segment?: string | null;
	equal: boolean;
};
