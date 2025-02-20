import { UmbMergeContentVariantDataController } from '../controller/merge-content-variant-data.controller.js';
import type { UmbElementDetailModel } from '../types.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbEntityWorkspaceDataManager, type UmbWorkspaceDataManager } from '@umbraco-cms/backoffice/workspace';

export class UmbElementWorkspaceDataManager<ModelType extends UmbElementDetailModel>
	extends UmbEntityWorkspaceDataManager<ModelType>
	implements UmbWorkspaceDataManager<ModelType>
{
	protected _varies?: boolean;
	//#variesByCulture?: boolean;
	//#variesBySegment?: boolean;

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

		const data = this._current.getValue();
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
