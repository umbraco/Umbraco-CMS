import type { UmbContentLikeDetailModel, UmbPotentialContentValueModel } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId, type UmbVariantDataModel } from '@umbraco-cms/backoffice/variant';

/**
 * @function defaultCompareVariantMethod
 * @param {UmbVariantDataModel} a - the first variant to compare.
 * @param {UmbVariantDataModel} b - the second variant to compare.
 * @returns {boolean} - true if the two models are equally unique.
 */
function defaultCompareVariantMethod(a: UmbVariantDataModel, b: UmbVariantDataModel) {
	return a.culture === b.culture && a.segment === b.segment;
}

export class UmbMergeContentVariantDataController extends UmbControllerBase {
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
		);

		if (currentData.variants) {
			// Notice for variants we do not want to include all the variants that we are processing. but just the once selected for the process. (Not include invariant if we are in a variant document) [NL]
			result.variants = this.#processVariants(
				persistedData?.variants,
				currentData.variants,
				selectedVariants,
				defaultCompareVariantMethod,
			);
		}

		this.destroy();

		return result;
	}

	/**
	 * Builds and saves values based on selected variants and variants to store.
	 * @param {Array<UmbPotentialContentValueModel> | undefined} persistedValues - The persisted values.
	 * @param {Array<UmbPotentialContentValueModel> | undefined} draftValues - The draft values.
	 * @param {Array<UmbVariantId>}variantsToStore - The variants to store.
	 * @returns {Promise<Array<UmbPotentialContentValueModel>>} - A promise that resolves to the saved values.
	 */
	async #processValues<T extends UmbPotentialContentValueModel = UmbPotentialContentValueModel>(
		persistedValues: Array<T> | undefined,
		draftValues: Array<T> | undefined,
		variantsToStore: Array<UmbVariantId>,
	): Promise<Array<T>> {
		// Make array of unique values, based on persistedValues and draftValues. Both alias, culture and segment has to be taken into account. [NL]

		const uniqueValues = [...(persistedValues ?? []), ...(draftValues ?? [])].filter(
			(n, i, self) =>
				i === self.findIndex((v) => v.alias === n.alias && v.culture === n.culture && v.segment === n.segment),
		);

		// Map unique values to their respective draft values.
		return (
			await Promise.all(
				uniqueValues.map((value) => {
					const persistedValue = persistedValues?.find(
						(x) => x.alias === value.alias && x.culture === value.culture && x.segment === value.segment,
					);

					// Should this value be saved?
					if (variantsToStore.some((x) => x.equal(UmbVariantId.CreateFromPartial(value)))) {
						const draftValue = draftValues?.find(
							(x) => x.alias === value.alias && x.culture === value.culture && x.segment === value.segment,
						);

						return this.#processValue(persistedValue, draftValue, variantsToStore);
					} else {
						// TODO: Check if this promise is needed: [NL]
						return Promise.resolve(persistedValue);
					}
				}),
			)
		).filter((x) => x !== undefined) as Array<T>;
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
			console.error(`Editor alias not found for ${editorAlias}`);
			return draftValue;
		}
		if (!draftValue) {
			// If the draft value does not exists then no need to process.
			return undefined;
		}

		// Find the resolver for this editor alias:
		const manifest = umbExtensionsRegistry.getByTypeAndFilter(
			'propertyValueResolver',
			// TODO: Remove depcrated filter in v.17 [NL]
			(x) => x.forEditorAlias === editorAlias || x.meta?.editorAlias === editorAlias,
		)[0];

		if (!manifest) {
			// No resolver found, then we can continue using the draftValue as is.
			return draftValue;
		}

		const api = await createExtensionApi(this, manifest);
		if (!api) {
			// If api is not to be found, then we can continue using the draftValue as is.
			return draftValue;
		}

		let newValue = draftValue;

		if (api.processValues) {
			// The a property values resolver resolves one value, we need to gather the persisted inner values first, and store them here:
			const persistedValuesHolder: Array<Array<UmbPotentialContentValueModel>> = [];

			if (persistedValue) {
				await api.processValues(persistedValue, async (values) => {
					persistedValuesHolder.push(values as unknown as Array<UmbPotentialContentValueModel>);
					return undefined;
				});
			}

			let valuesIndex = 0;
			newValue =
				(await api.processValues(newValue, async (values) => {
					// got some values (content and/or settings):
					// but how to get the persisted and the draft of this.....
					const persistedValues = persistedValuesHolder[valuesIndex++];

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
						api.compareVariants ?? defaultCompareVariantMethod,
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
	 * @returns {UmbVariantDataModel[]} A new array of variants.
	 */
	#processVariants<VariantModel extends UmbVariantDataModel = UmbVariantDataModel>(
		persistedVariants: Array<VariantModel> | undefined,
		draftVariants: Array<VariantModel>,
		variantsToStore: Array<UmbVariantId>,
		compare: (a: VariantModel, b: VariantModel) => boolean,
	): Array<VariantModel> {
		const uniqueVariants = [...(persistedVariants ?? []), ...(draftVariants ?? [])].filter(
			(n, i, self) => i === self.findIndex((v) => compare(v, n)),
		);

		return uniqueVariants
			.map((value) => {
				const persistedVariant = persistedVariants?.find((x) => compare(x, value));

				// Should this value be saved?
				if (variantsToStore.some((x) => x.compare(value))) {
					const draftVariant = draftVariants?.find((x) => compare(x, value));

					return draftVariant;
				} else {
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
}
