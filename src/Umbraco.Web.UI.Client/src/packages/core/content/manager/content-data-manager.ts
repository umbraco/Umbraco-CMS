import type { UmbContentDetailModel } from '../types.js';
import { UmbElementWorkspaceDataManager } from './element-data-manager.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { appendToFrozenArray, jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId, umbVariantObjectCompare, type UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

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
						persistedVariants.findIndex((x) => umbVariantObjectCompare(x, a)) -
						persistedVariants.findIndex((x) => umbVariantObjectCompare(x, b))
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

	updateVariantData(variantId: UmbVariantId, update?: Partial<ModelVariantType>) {
		const currentData = this.getCurrent();
		if (!currentData) throw new Error('Data is missing');
		if (!this.#variantScaffold) throw new Error('Variant scaffold data is missing');
		if (this._varies === true) {
			// If variant Id is invariant, we don't to have the variant appended to our data.
			if (variantId.isInvariant()) return;
			const variant = currentData.variants.find((x) => variantId.compare(x));
			const newVariants = appendToFrozenArray(
				currentData.variants,
				{
					...this.#variantScaffold,
					...variantId.toObject(),
					...variant,
					...update,
				} as ModelVariantType,
				(x) => variantId.compare(x),
			) as Array<ModelVariantType>;
			this.updateCurrent({ variants: newVariants } as unknown as ModelType);
		} else if (this._varies === false) {
			// TODO: Beware about segments, in this case we need to also consider segments, if its allowed to vary by segments.
			const invariantVariantId = UmbVariantId.CreateInvariant();
			const variant = currentData.variants.find((x) => invariantVariantId.compare(x));
			// Cause we are invariant, we will just overwrite all variants with this one:
			const newVariants = [
				{
					...this.#variantScaffold,
					...invariantVariantId.toObject(),
					...variant,
					...update,
				} as ModelVariantType,
			];
			this.updateCurrent({ variants: newVariants } as unknown as ModelType);
		} else {
			throw new Error('Varies by culture is missing');
		}
	}

	getChangedVariants() {
		const persisted = this.getPersisted();
		const current = this.getCurrent();
		if (!current) throw new Error('Current data is missing');

		const changedVariants = current?.variants.map((variant) => {
			const persistedVariant = persisted?.variants.find((x) => UmbVariantId.Create(variant).compare(x));
			return {
				culture: variant.culture,
				segment: variant.segment,
				equal: persistedVariant ? jsonStringComparison(variant, persistedVariant) : false,
			};
		});

		const changedProperties = current?.values.map((value) => {
			const persistedValues = persisted?.values.find((x) => UmbVariantId.Create(value).compare(x));
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
