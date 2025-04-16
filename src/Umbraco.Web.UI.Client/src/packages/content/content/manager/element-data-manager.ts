import { UmbMergeContentVariantDataController } from '../controller/merge-content-variant-data.controller.js';
import type { UmbElementDetailModel } from '../types.js';
import { UmbVariantId, umbVariantObjectCompare } from '@umbraco-cms/backoffice/variant';
import { UmbEntityWorkspaceDataManager, type UmbWorkspaceDataManager } from '@umbraco-cms/backoffice/workspace';

/**
 *
 * @param a
 * @param b
 */
function valueObjectCompare(a: any, b: any) {
	return a.alias === b.alias && umbVariantObjectCompare(a, b);
}

export class UmbElementWorkspaceDataManager<ModelType extends UmbElementDetailModel>
	extends UmbEntityWorkspaceDataManager<ModelType>
	implements UmbWorkspaceDataManager<ModelType>
{
	protected _varies?: boolean;
	protected _variesByCulture?: boolean;
	protected _variesBySegment?: boolean;

	protected override _sortCurrentData<GivenType extends Partial<ModelType> = Partial<ModelType>>(
		persistedData: Partial<ModelType>,
		currentData: GivenType,
	): GivenType {
		currentData = super._sortCurrentData(persistedData, currentData);
		// Sort the values in the same order as the persisted data:
		const persistedValues = persistedData.values;
		if (persistedValues && currentData.values) {
			return {
				...currentData,
				values: [...currentData.values].sort(function (a, b) {
					return (
						persistedValues.findIndex((x) => valueObjectCompare(x, a)) -
						persistedValues.findIndex((x) => valueObjectCompare(x, b))
					);
				}),
			};
		}
		return currentData;
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

	setVariesByCulture(vary: boolean | undefined) {
		this._variesByCulture = vary;
	}

	setVariesBySegment(vary: boolean | undefined) {
		this._variesBySegment = vary;
	}

	setVaries(vary: boolean | undefined) {
		this._varies = vary;
	}

	async constructData(selectedVariants: Array<UmbVariantId>): Promise<ModelType> {
		// Lets correct the selected variants, so invariant is included, or the only one if invariant.
		// TODO: VDIVD: Could a document be set to invariant but hold variant data inside it?
		const invariantVariantId = UmbVariantId.CreateInvariant();
		let variantsToStore = [invariantVariantId];
		if (this._varies === false) {
			// If we do not vary, we wil just pick the invariant variant id.
			selectedVariants = [invariantVariantId];
		} else {
			variantsToStore = [...selectedVariants, invariantVariantId];
		}

		// If we vary by segment we need to save all segments for a selected culture.
		if (this._variesBySegment === true) {
			const dataSegments = this.getCurrent()!.values.map((x) => x.segment);
			variantsToStore = [
				...variantsToStore,
				...dataSegments.flatMap((segment) => variantsToStore.map((variant) => variant.toSegment(segment))),
			];
		}

		const data = this.getCurrent();
		if (!data) throw new Error('Current data is missing');
		//if (!data.unique) throw new Error('Unique of current data is missing');

		const persistedData = this.getPersisted();

		return await new UmbMergeContentVariantDataController(this).process(
			persistedData,
			data,
			selectedVariants,
			variantsToStore,
		);
	}
}
