import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbContentDetailModel, UmbPotentialContentValueModel } from '@umbraco-cms/backoffice/content';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbVariantId, type UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';

export class UmbMergeContentVariantDataController extends UmbControllerBase {
	/**
	 * Merges content variant data based on selected variants and variants to store.
	 * @param {UmbContentDetailModel | undefined} persistedData - The persisted content variant data.
	 * @param {UmbContentDetailModel} currentData - The current content variant data.
	 * @param {Array<UmbVariantId>} selectedVariants - The selected variants.
	 * @param {Array<UmbVariantId>} variantsToStore - The variants to store.
	 * @returns A promise that resolves to the merged content variant data.
	 */
	async process<ModelType extends UmbContentDetailModel>(
		persistedData: ModelType | undefined,
		currentData: ModelType,
		selectedVariants: Array<UmbVariantId>,
		variantsToStore: Array<UmbVariantId>,
	): Promise<ModelType> {
		// Combine data and persisted data depending on the selectedVariants. Always use the invariant values from the data.
		// loops over each entry in values, determine wether the value should be from the data or the persisted data, depending on wether its a selectedVariant or an invariant value.
		// loops over each entry in variants, determine wether the variant should be from the data or the persisted data, depending on the selectedVariants.
		const result = {
			...currentData,
			values: await this.#buildSaveValues<ModelType['values'][0]>(
				persistedData?.values,
				currentData.values,
				selectedVariants,
				variantsToStore,
			),
			variants: this.#buildSaveVariants(persistedData?.variants, currentData.variants, selectedVariants),
		};

		this.destroy();

		return result;
	}

	/**
	 * Builds and saves values based on selected variants and variants to store.
	 * @param {Array<UmbPotentialContentValueModel> | undefined} persistedValues - The persisted values.
	 * @param {Array<UmbPotentialContentValueModel> | undefined} draftValues - The draft values.
	 * @param {Array<UmbVariantId>} selectedVariants - The selected variants.
	 * @param {Array<UmbVariantId>}variantsToStore - The variants to store.
	 * @returns A promise that resolves to the saved values.
	 */
	async #buildSaveValues<T extends UmbPotentialContentValueModel = UmbPotentialContentValueModel>(
		persistedValues: Array<T> | undefined,
		draftValues: Array<T> | undefined,
		selectedVariants: Array<UmbVariantId>,
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

						return this.#buildSaveValue(persistedValue, draftValue, selectedVariants, variantsToStore);
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
	 * @param {Array<UmbVariantId>} selectedVariants - The selected variants.
	 * @param {Array<UmbVariantId>} variantsToStore - The variants to store.
	 * @returns {Promise<UmbPotentialContentValueModel | undefined>} A promise that resolves to the saved value.
	 */
	async #buildSaveValue(
		persistedValue: UmbPotentialContentValueModel | undefined,
		draftValue: UmbPotentialContentValueModel | undefined,
		selectedVariants: Array<UmbVariantId>,
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
			(x) => x.meta.editorAlias === editorAlias,
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
			newValue = await api.processValues(newValue, async (values) => {
				// got some values (content and/or settings):
				// but how to get the persisted and the draft of this.....
				const persistedValues = persistedValuesHolder[valuesIndex++];

				return await this.#buildSaveValues(persistedValues, values, selectedVariants, variantsToStore);
			});
		}

		if (api.ensureVariants) {
			// The a property values resolver resolves one value, we need to gather the persisted inner values first, and store them here:
			//const persistedVariants = newValue ? ((await api.readVariants(newValue)) ?? []) : [];

			const args = {
				selectedVariants,
			};
			newValue = await api.ensureVariants(newValue, args);
		}

		// the api did not provide a value processor, so we will return the draftValue:
		return newValue;
	}

	/**
	 * Construct variants property of model.
	 * @param {Array<UmbEntityVariantModel> | undefined} persistedVariants - The persisted value.
	 * @param {Array<UmbEntityVariantModel>} draftVariants - The draft value.
	 * @param {Array<UmbVariantId>} selectedVariants - The selected variants.
	 * @returns {UmbEntityVariantModel[]} A new array of variants.
	 */
	#buildSaveVariants(
		persistedVariants: Array<UmbEntityVariantModel> | undefined,
		draftVariants: Array<UmbEntityVariantModel>,
		selectedVariants: Array<UmbVariantId>,
	) {
		return draftVariants
			.map((variant) => {
				// Should this value be saved?
				if (selectedVariants.some((x) => x.compare(variant))) {
					return variant;
				} else {
					// If not we will find the value in the persisted data and use that instead.
					return persistedVariants?.find((x) => x.culture === variant.culture && x.segment === variant.segment);
				}
			})
			.filter((x) => x !== undefined) as Array<UmbEntityVariantModel>;
	}
}
