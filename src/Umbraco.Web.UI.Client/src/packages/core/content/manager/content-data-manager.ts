import { UmbMergeContentVariantDataController } from '../controller/merge-content-variant-data.controller.js';
import type { UmbContentDetailModel } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { appendToFrozenArray, jsonStringComparison } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId, type UmbEntityVariantModel } from '@umbraco-cms/backoffice/variant';
import { UmbEntityWorkspaceDataManager, type UmbWorkspaceDataManager } from '@umbraco-cms/backoffice/workspace';

export class UmbContentWorkspaceDataManager<
		ModelType extends UmbContentDetailModel,
		ModelVariantType extends UmbEntityVariantModel = ModelType extends { variants: UmbEntityVariantModel[] }
			? ModelType['variants'][0]
			: never,
	>
	extends UmbEntityWorkspaceDataManager<ModelType>
	implements UmbWorkspaceDataManager<ModelType>
{
	//
	//#repository;
	#variantScaffold?: ModelVariantType;

	#varies?: boolean;
	//#variesByCulture?: boolean;
	//#variesBySegment?: boolean;

	constructor(host: UmbControllerHost, variantScaffold: ModelVariantType) {
		super(host);
		this.#variantScaffold = variantScaffold;
	}

	#updateLock = 0;
	initiatePropertyValueChange() {
		this.#updateLock++;
		this._current.mute();
		// TODO: When ready enable this code will enable handling a finish automatically by this implementation 'using myState.initiatePropertyValueChange()' (Relies on TS support of Using) [NL]
		/*return {
			[Symbol.dispose]: this.finishPropertyValueChange,
		};*/
	}
	finishPropertyValueChange = () => {
		this.#updateLock--;
		this.#triggerPropertyValueChanges();
	};
	#triggerPropertyValueChanges() {
		if (this.#updateLock === 0) {
			this._current.unmute();
		}
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

	ensureVariantData(variantId: UmbVariantId) {
		this.updateVariantData(variantId);
	}

	updateVariantData(variantId: UmbVariantId, update?: Partial<ModelVariantType>) {
		const currentData = this.getCurrent();
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
			this._current.update({ variants: newVariants } as any);
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
			this._current.update({ variants: newVariants } as any);
		} else {
			throw new Error('Varies by culture is missing');
		}
	}

	async constructData(selectedVariants: Array<UmbVariantId>): Promise<ModelType> {
		// Lets correct the selected variants, so invariant is included, or the only one if invariant.
		// TODO: VDIVD: Could a document be set to invariant but hold variant data inside it?
		const invariantVariantId = UmbVariantId.CreateInvariant();
		let variantsToStore = [invariantVariantId];
		if (this.#varies === false) {
			// If we do not vary, we wil just pick the invariant variant id.
			selectedVariants = [invariantVariantId];
		} else {
			variantsToStore = [...selectedVariants, invariantVariantId];
		}

		const data = this._current.getValue();
		if (!data) throw new Error('Current data is missing');
		if (!data.unique) throw new Error('Unique of current data is missing');

		const persistedData = this.getPersisted();

		return await new UmbMergeContentVariantDataController(this).process(
			persistedData,
			data,
			selectedVariants,
			variantsToStore,
		);
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
