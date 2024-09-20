import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbContentDetailModel, UmbPotentialContentValueModel } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbObjectState, appendToFrozenArray, jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import type { UmbDetailRepository } from '@umbraco-cms/backoffice/repository';
import { UmbVariantId, type UmbVariantModel } from '@umbraco-cms/backoffice/variant';

export class UmbContentDataManager<
	ModelType extends UmbContentDetailModel,
	ModelVariantType extends UmbVariantModel = ModelType['variants'][0],
> extends UmbControllerBase {
	//
	//#repository;
	#variantScaffold?: ModelVariantType;

	#persisted = new UmbObjectState<ModelType | undefined>(undefined);
	readonly current = new UmbObjectState<ModelType | undefined>(undefined);

	#varies?: boolean;
	//#variesByCulture?: boolean;
	//#variesBySegment?: boolean;

	constructor(host: UmbControllerHost, repository: UmbDetailRepository<ModelType>) {
		super(host);
		//this.#repository = repository;
		repository
			.createScaffold()
			.then((x) => (this.#variantScaffold = x.data?.variants[0] as ModelVariantType | undefined));
	}

	setData(incomingData: ModelType | undefined) {
		this.#persisted.setValue(incomingData);
		this.current.setValue(incomingData);
	}

	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	setVariesByCulture(vary: boolean | undefined) {
		//this.#variesByCulture = vary;
	}
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	setVariesBySegment(vary: boolean | undefined) {
		//this.#variesBySegment = vary;
	}
	setVaries(vary: boolean | undefined) {
		this.#varies = vary;
	}

	getPersistedData() {
		return this.#persisted.getValue();
	}

	getCurrentData() {
		return this.current.getValue();
	}

	ensureVariantData(variantId: UmbVariantId) {
		this.updateVariantData(variantId);
	}

	updateVariantData(variantId: UmbVariantId, update?: Partial<ModelVariantType>) {
		const currentData = this.current.getValue();
		if (!currentData) throw new Error('Data is missing');
		if (!this.#variantScaffold) throw new Error('Variant scaffold data is missing');
		if (this.#varies === true) {
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
			// TODO: I have some trouble with TypeScript here, I does not look like me, but i had to give up. [NL]
			this.current.update({ variants: newVariants } as any);
		} else if (this.#varies === false) {
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
			// TODO: I have some trouble with TypeScript here, I does not look like me, but i had to give up. [NL]
			this.current.update({ variants: newVariants } as any);
		} else {
			throw new Error('Varies by culture is missing');
		}
	}

	async constructData(selectedVariants: Array<UmbVariantId>): Promise<ModelType> {
		// Lets correct the selected variants, so invariant is included, or the only one if invariant.
		// TODO: VDIVD: Could a document be set to invariant but hold variant data inside it?
		const invariantVariantId = UmbVariantId.CreateInvariant();
		if (this.#varies === false) {
			// If we do not vary, we wil just pick the invariant variant id.
			selectedVariants = [invariantVariantId];
		} else {
			selectedVariants = [...selectedVariants, invariantVariantId];
		}

		const data = this.current.getValue();
		if (!data) throw new Error('Data is missing');
		if (!data.unique) throw new Error('Unique is missing');

		const persistedData = this.getPersistedData();

		// Combine data and persisted data depending on the selectedVariants. Always use the invariant values from the data.
		// loops over each entry in values, determine wether the value should be from the data or the persisted data, depending on wether its a selectedVariant or an invariant value.
		// loops over each entry in variants, determine wether the variant should be from the data or the persisted data, depending on the selectedVariants.
		const result = {
			...data,
			values: await this.#buildSaveValues<UmbPotentialContentValueModel>(
				persistedData?.values,
				data.values,
				selectedVariants,
			),
			variants: this.#buildSaveVariants(persistedData?.variants, data.variants, selectedVariants),
		};

		return result;
	}

	async #buildSaveValues<T extends UmbPotentialContentValueModel = UmbPotentialContentValueModel>(
		persistedValues: Array<T> | undefined,
		draftValues: Array<T> | undefined,
		selectedVariants: Array<UmbVariantId>,
	): Promise<Array<T>> {
		// Make array of unique values, based on persistedValues and draftValues. unique values are based upon alias culture and segment
		const uniqueValues = [
			...new Set(
				[...(persistedValues ?? []), ...(draftValues ?? [])].map((x) => ({
					alias: x.alias,
					culture: x.culture,
					segment: x.segment,
				})),
			),
		];

		// Map unique values to their respective draft values.
		return await Promise.all(
			uniqueValues
				.map((value) => {
					const persistedValue = persistedValues?.find(
						(x) => x.alias === value.alias && x.culture === value.culture && x.segment === value.segment,
					);
					// Should this value be saved?
					if (selectedVariants.some((x) => x.equal(UmbVariantId.CreateFromPartial(value)))) {
						const draftValue = draftValues?.find(
							(x) => x.alias === value.alias && x.culture === value.culture && x.segment === value.segment,
						);

						return this.#buildSaveValue(persistedValue, draftValue, selectedVariants);
					} else {
						// TODO: Check if this promise is needed: [NL]
						return Promise.resolve(persistedValue);
					}
				})
				.filter((x) => x !== undefined) as Array<Promise<T>>,
		);
	}

	async #buildSaveValue(
		persistedValue: UmbPotentialContentValueModel | undefined,
		draftValue: UmbPotentialContentValueModel | undefined,
		selectedVariants: Array<UmbVariantId>,
	): Promise<UmbPotentialContentValueModel | undefined> {
		const editorAlias = draftValue?.editorAlias ?? persistedValue?.editorAlias;
		if (!editorAlias) {
			console.error(`Editor alias not found for ${editorAlias}`);
			return draftValue;
		}
		if (!persistedValue) {
			// If the persisted value does not exists then no need to combine.
			return draftValue;
		}
		if (!draftValue) {
			// If the draft value does not exists then no need to combine.
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

		if (api.processValues) {
			// The a property values resolver resolves one value, we need to gather the persisted inner values first, and store them here:
			const persistedValuesHolder: Array<Array<UmbPotentialContentValueModel>> = [];

			await api.processValues(persistedValue, async (values) => {
				persistedValuesHolder.push(values as unknown as Array<UmbPotentialContentValueModel>);
				return undefined;
			});

			let valuesIndex = 0;
			return await api.processValues(draftValue, async (values) => {
				// got some values (content and/or settings):
				// but how to get the persisted and the draft of this.....
				const persistedValues = persistedValuesHolder[valuesIndex++];

				return await this.#buildSaveValues(persistedValues, values, selectedVariants);
			});
		}

		// the api did not provide a value processor, so we will return the draftValue:
		return draftValue;
	}

	#buildSaveVariants(
		persistedVariants: Array<UmbVariantModel> | undefined,
		draftVariants: Array<UmbVariantModel>,
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
			.filter((x) => x !== undefined) as Array<UmbVariantModel>;
	}

	getChangedVariants() {
		const persisted = this.#persisted.getValue();
		const current = this.current.getValue();
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

	public override destroy(): void {
		this.#persisted.destroy();
		this.current.destroy();
		super.destroy();
	}
}
