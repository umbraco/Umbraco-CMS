import type { UmbContentLikeDetailModel, UmbPotentialContentValueModel } from '../types.js';
import { _sortContentValuesByVariant } from '../utils/sort-content-values-by-variant.function.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsManifestCache } from '@umbraco-cms/backoffice/extension-registry';
import { UmbPropertyValueIndex, type ManifestPropertyValueResolver } from '@umbraco-cms/backoffice/property';
import {
	umbVariantObjectCompare,
	umbVariantObjectSortCompare,
	type UmbVariantDataModel,
	type UmbVariantId,
} from '@umbraco-cms/backoffice/variant';

const variantObjectKey = (variant: UmbVariantDataModel) => `${variant.culture}|${variant.segment}`;

export class UmbMergeContentVariantDataController extends UmbControllerBase {
	#resolvers = new UmbExtensionsManifestCache<ManifestPropertyValueResolver>(
		this,
		'propertyValueResolver',
		(manifest, editorAlias) => manifest.forEditorAlias === editorAlias,
	);
	#persistedValuesIndex?: UmbPropertyValueIndex<UmbPotentialContentValueModel>;
	#draftValuesIndex?: UmbPropertyValueIndex<UmbPotentialContentValueModel>;

	/**
	 * Supplies a prebuilt index over the persisted `values` passed to {@link process}, so it does not have to build its own.
	 * Useful when several controllers merge against the same persisted data. The index must have been built from exactly
	 * those values; no check is made. It applies to the top-level values only, not to values nested inside property values.
	 * @param {UmbPropertyValueIndex<UmbPotentialContentValueModel>} index - An index over the persisted `values`.
	 */
	setPersistedValuesIndex(index: UmbPropertyValueIndex<UmbPotentialContentValueModel>): void {
		this.#persistedValuesIndex = index;
	}

	/**
	 * Supplies a prebuilt index over the draft (current) `values` passed to {@link process}, so it does not have to build its own.
	 * Useful when several controllers merge the same draft data. The index must have been built from exactly
	 * those values; no check is made. It applies to the top-level values only, not to values nested inside property values.
	 * @param {UmbPropertyValueIndex<UmbPotentialContentValueModel>} index - An index over the draft `values`.
	 */
	setDraftValuesIndex(index: UmbPropertyValueIndex<UmbPotentialContentValueModel>): void {
		this.#draftValuesIndex = index;
	}

	/**
	 * Merges content variant data based on selected variants and variants to store.
	 * @param {UmbContentLikeDetailModel | undefined} persistedData - The persisted content variant data.
	 * @param {UmbContentLikeDetailModel} currentData - The current content variant data.
	 * @param {Array<UmbVariantId>} selectedVariants - The selected variants.
	 * @param {Array<UmbVariantId>} variantsToStore - The variants to store, we sometimes have additional variants that we like to process. This is typically the invariant variant, which we do not want to have as part of the variants data therefore a difference.
	 * @returns {Promise<UmbContentLikeDetailModel>} - A promise that resolves to the merged content variant data.
	 */
	async process<ModelType extends UmbContentLikeDetailModel>(
		persistedData: ModelType | undefined,
		currentData: ModelType,
		selectedVariants: Array<UmbVariantId>,
		variantsToStore: Array<UmbVariantId>,
	): Promise<ModelType> {
		// Combine data and persisted data depending on the selectedVariants. Always use the invariant values from the data.
		// loops over each entry in values, determine wether the value should be from the data or the persisted data, depending on wether its a selectedVariant or an invariant value.
		// loops over each entry in variants, determine wether the variant should be from the data or the persisted data, depending on the selectedVariants.
		const result = { ...currentData };
		result.values = await this.#processValues<ModelType['values'][0]>(
			persistedData?.values,
			currentData.values,
			variantsToStore,
			this.#persistedValuesIndex,
			this.#draftValuesIndex,
		);

		if (currentData.variants) {
			// Notice for variants we do not want to include all the variants that we are processing. but just the once selected for the process. (Not include invariant if we are in a variant document) [NL]
			result.variants = this.#processVariants(
				persistedData?.variants,
				currentData.variants,
				selectedVariants,
				umbVariantObjectCompare,
				variantObjectKey,
			).sort(umbVariantObjectSortCompare);
		}

		this.destroy();

		return result;
	}

	/**
	 * Builds and saves values based on selected variants and variants to store.
	 * @param {Array<UmbPotentialContentValueModel> | undefined} persistedValues - The persisted values.
	 * @param {Array<UmbPotentialContentValueModel> | undefined} draftValues - The draft values.
	 * @param {Array<UmbVariantId>}variantsToStore - The variants to store.
	 * @param {UmbPropertyValueIndex<UmbPotentialContentValueModel>} [suppliedPersistedIndex] - A prebuilt index over `persistedValues`, built here when omitted.
	 * @param {UmbPropertyValueIndex<UmbPotentialContentValueModel>} [suppliedDraftIndex] - A prebuilt index over `draftValues`, built here when omitted.
	 * @returns {Promise<Array<UmbPotentialContentValueModel>>} - A promise that resolves to the saved values.
	 */
	async #processValues<T extends UmbPotentialContentValueModel = UmbPotentialContentValueModel>(
		persistedValues: Array<T> | undefined,
		draftValues: Array<T> | undefined,
		variantsToStore: Array<UmbVariantId>,
		suppliedPersistedIndex?: UmbPropertyValueIndex<UmbPotentialContentValueModel>,
		suppliedDraftIndex?: UmbPropertyValueIndex<UmbPotentialContentValueModel>,
	): Promise<Array<T>> {
		const persistedIndex = suppliedPersistedIndex ?? new UmbPropertyValueIndex(persistedValues ?? []);
		const draftIndex = suppliedDraftIndex ?? new UmbPropertyValueIndex(draftValues ?? []);

		// Make array of unique values, based on persistedValues and draftValues. Both alias, culture and segment has to be taken into account. [NL]
		// Each list is unique on its own, so a draft value only takes part when it is not already persisted.
		const uniqueValues = [
			...(persistedValues ?? []),
			...(draftValues ?? []).filter((value) => persistedIndex.find(value.alias, value) === undefined),
		];

		// Map unique values to their respective draft values.
		const values = (
			await Promise.all(
				uniqueValues.map((value) => {
					const persistedValue = persistedIndex.find(value.alias, value);

					// Should this value be saved?
					if (variantsToStore.some((x) => x.compare(value))) {
						const draftValue = draftIndex.find(value.alias, value);

						return this.#processValue(persistedValue, draftValue, variantsToStore);
					} else {
						// TODO: Check if this promise is needed: [NL]
						return Promise.resolve(persistedValue);
					}
				}),
			)
		).filter((x) => x !== undefined) as Array<T>;

		return _sortContentValuesByVariant(values);
	}

	/**
	 * Builds and saves a value based on selected variants and variants to store.
	 * @param {UmbPotentialContentValueModel | undefined} persistedValue - The persisted value.
	 * @param {UmbPotentialContentValueModel | undefined} draftValue - The draft value.
	 * @param {Array<UmbVariantId>} variantsToStore - The variants to store.
	 * @returns {Promise<UmbPotentialContentValueModel | undefined>} A promise that resolves to the saved value.
	 */
	async #processValue(
		persistedValue: UmbPotentialContentValueModel | undefined,
		draftValue: UmbPotentialContentValueModel | undefined,
		variantsToStore: Array<UmbVariantId>,
	): Promise<UmbPotentialContentValueModel | undefined> {
		const editorAlias = draftValue?.editorAlias ?? persistedValue?.editorAlias;
		if (!editorAlias) {
			console.error(`Editor alias not found for ${editorAlias}`, draftValue, persistedValue);
			return draftValue;
		}
		if (!draftValue) {
			// If the draft value does not exists then no need to process.
			return undefined;
		}

		const manifest = this.#resolvers.getManifests(editorAlias)[0];
		if (!manifest) {
			// No resolver found, then we can continue using the draftValue as is.
			return draftValue;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			// If api is not to be found, then we can continue using the draftValue as is.
			return draftValue;
		}

		// TODO: Setting the manifest on the api is legacy, the implementation should not have to do this. [NL]
		api.manifest = manifest;

		let newValue = draftValue;

		if (api.processValues) {
			// A resolver may emit several groups of inner values.
			// Pair each draft group with its persisted group by the
			// identifier the resolver supplies, not by call order.
			const persistedInnerValues = new Map<string, Array<UmbPotentialContentValueModel>>();
			let persistedFallbackIndex = 0;

			if (persistedValue) {
				await api.processValues(persistedValue, async (values, identifier) => {
					persistedInnerValues.set(
						// Resolvers that do not supply an identifier will fallback to call order.
						identifier ?? (persistedFallbackIndex++).toString(),
						values as unknown as Array<UmbPotentialContentValueModel>,
					);
					return undefined;
				});
			}

			let valuesFallbackIndex = 0;
			newValue =
				(await api.processValues(newValue, async (values, identifier) => {
					// Resolvers that do not supply an identifier will fallback to call order.
					const persistedValues = persistedInnerValues.get(identifier ?? (valuesFallbackIndex++).toString());

					return await this.#processValues(persistedValues, values, variantsToStore);
				})) ?? newValue;
		}

		if (api.processVariants) {
			// The a property values resolver resolves one value, we need to gather the persisted inner values first, and store them here:
			const persistedVariantsHolder: Array<Array<UmbVariantDataModel>> = [];

			if (persistedValue) {
				await api.processVariants(persistedValue, async (values) => {
					persistedVariantsHolder.push(values as unknown as Array<UmbVariantDataModel>);
					return undefined;
				});
			}

			let valuesIndex = 0;
			newValue =
				(await api.processVariants(newValue, async (values) => {
					// got some values (content and/or settings):
					// but how to get the persisted and the draft of this.....
					const persistedVariants = persistedVariantsHolder[valuesIndex++];

					return await this.#processVariants(
						persistedVariants,
						values,
						variantsToStore,
						api.compareVariants ?? umbVariantObjectCompare,
						api.compareVariants ? api.variantKey : variantObjectKey,
					);
				})) ?? newValue;
		}

		/*
		if (api.ensureVariants) {
			// The a property values resolver resolves one value, we need to gather the persisted inner values first, and store them here:
			//const persistedVariants = newValue ? ((await api.readVariants(newValue)) ?? []) : [];

			// TODO: An expose for a Block should be invariant if the Block Content Element Type is not vary by culture.
			// TODO: And expose determination should look for invariant expose in this case.
			const args = {
				selectedVariants,
			};
			newValue = await api.ensureVariants(newValue, args);
		}
			*/

		// the api did not provide a value processor, so we will return the draftValue:
		return newValue;
	}

	/**
	 * Construct variants property of model.
	 * @param {Array<UmbVariantDataModel> | undefined} persistedVariants - The persisted value.
	 * @param {Array<UmbVariantDataModel>} draftVariants - The draft value.
	 * @param {Array<UmbVariantId>} variantsToStore - The variants to be stored.
	 * @param {(UmbVariantDataModel, UmbVariantDataModel) => boolean} compare - The compare method, which compares the unique properties of the variants.
	 * @param {(UmbVariantDataModel) => string} [key] - Returns a key that is equal exactly when `compare` considers two variants equal. When given, variants are matched by lookup instead of comparing every pair.
	 * @returns {UmbVariantDataModel[]} A new array of variants.
	 */
	#processVariants<VariantModel extends UmbVariantDataModel = UmbVariantDataModel>(
		persistedVariants: Array<VariantModel> | undefined,
		draftVariants: Array<VariantModel>,
		variantsToStore: Array<UmbVariantId>,
		compare: (a: VariantModel, b: VariantModel) => boolean,
		key?: (variant: VariantModel) => string,
	): Array<VariantModel> {
		if (key) {
			return this.#processVariantsByKey(persistedVariants, draftVariants, variantsToStore, key);
		}

		const uniqueVariants = [...(persistedVariants ?? []), ...(draftVariants ?? [])].filter(
			(n, i, self) => i === self.findIndex((v) => compare(v, n)),
		);

		// Each variant is taken from the draft when its culture and segment are being stored, otherwise from the
		// persisted data. When the side it should come from does not have it, it is left out.
		// When a list holds the same variant twice the first one wins.
		// The result lists persisted variants first, then the draft-only ones.
		return uniqueVariants
			.map((value) => {
				const persistedVariant = persistedVariants?.find((x) => compare(x, value));

		// 1. Every variant in the persisted data, skipping repeats of an earlier persisted variant:
		//    - being stored: use the first matching draft variant, or leave it out when the draft no longer has it.
		//    - not being stored: keep the persisted variant.
				// Should this value be saved?
				if (variantsToStore.some((x) => x.compare(value))) {
					const draftVariant = draftVariants?.find((x) => compare(x, value));

					return draftVariant;
				} else {

		// 2. Every variant only found in the draft, skipping repeats of an earlier draft variant:
		//    - being stored: add the draft variant.
		//    - not being stored: leave it out, as that draft change is not part of this merge.
		//    The cheap stored check runs first, so variants that are not stored never scan the lists.
					// TODO: Check if this promise is needed: [NL]
					return persistedVariant;
		}
			})
			.filter((x) => x !== undefined) as Array<VariantModel>;

		/*
		return draftVariants
			.map((variant) => {
				// Should this variant be saved?
				if (variantsToStore.some((x) => x.compare(variant))) {
					return variant;
				} else {
					// If not, then we will tru to find the variant in the persisted data and use that instead.
					return persistedVariants?.find((x) => x.culture === variant.culture && x.segment === variant.segment);
				}
			})
			.filter((x) => x !== undefined) as Array<VariantModel>;
			*/
	}

	#processVariantsByKey<VariantModel extends UmbVariantDataModel>(
		persistedVariants: Array<VariantModel> | undefined,
		draftVariants: Array<VariantModel>,
		variantsToStore: Array<UmbVariantId>,
		key: (variant: VariantModel) => string,
	): Array<VariantModel> {
		// Index both lists by key. When a list holds the same key twice the first one wins, and the maps
		// keep the order of the lists, so iterating them below follows the original order.
		const persistedByKey = this.#mapFirstByKey(persistedVariants ?? [], key);
		const draftByKey = this.#mapFirstByKey(draftVariants ?? [], key);

		// Each variant is taken from the draft when its culture and segment are being stored, otherwise from the
		// persisted data. When the side it should come from does not have it, it is left out.
		// The result keeps the order of the pairwise path: persisted variants first, then the draft-only ones.
		const result: Array<VariantModel> = [];

		// 1. Every variant in the persisted data:
		//    - being stored: use the draft variant with the same key, or leave it out when the draft no longer has it.
		//    - not being stored: keep the persisted variant.
		for (const [variantKey, variant] of persistedByKey) {
			const picked = variantsToStore.some((x) => x.compare(variant)) ? draftByKey.get(variantKey) : variant;
			if (picked) result.push(picked);
		}

		// 2. Every variant only found in the draft (keys present in the persisted data were handled in step 1):
		//    - being stored: add the draft variant.
		//    - not being stored: leave it out, as that draft change is not part of this merge.
		for (const [variantKey, variant] of draftByKey) {
			if (persistedByKey.has(variantKey)) continue;
			if (variantsToStore.some((x) => x.compare(variant))) result.push(variant);
		}
		return result;
	}

	#mapFirstByKey<VariantModel>(variants: Array<VariantModel>, key: (variant: VariantModel) => string) {
		const map = new Map<string, VariantModel>();
		for (const variant of variants) {
			const variantKey = key(variant);
			if (!map.has(variantKey)) map.set(variantKey, variant);
		}
		return map;
	}
}
