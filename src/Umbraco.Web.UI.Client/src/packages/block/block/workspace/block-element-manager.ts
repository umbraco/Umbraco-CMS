import type { UmbBlockDataModel, UmbBlockDataValueModel } from '../types.js';
import { UmbBlockElementPropertyDatasetContext } from './block-element-property-dataset.context.js';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	type Observable,
	UmbClassState,
	UmbObjectState,
	appendToFrozenArray,
	mergeObservables,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type UmbClassInterface, UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbValidationController } from '@umbraco-cms/backoffice/validation';

export class UmbBlockElementManager extends UmbControllerBase {
	//
	#data = new UmbObjectState<UmbBlockDataModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	#getDataPromise = new Promise<void>((resolve) => {
		this.#getDataResolver = resolve;
	});
	#getDataResolver!: () => void;

	#variantId = new UmbClassState<UmbVariantId | undefined>(undefined);
	readonly variantId = this.#variantId.asObservable();

	readonly unique = this.#data.asObservablePart((data) => data?.key);
	readonly contentTypeId = this.#data.asObservablePart((data) => data?.contentTypeKey);

	readonly structure = new UmbContentTypeStructureManager<UmbContentTypeModel>(
		this,
		new UmbDocumentTypeDetailRepository(this),
	);

	readonly validation = new UmbValidationController(this);

	constructor(host: UmbControllerHost, dataPathPropertyName: string) {
		super(host);

		this.observe(this.contentTypeId, (id) => this.structure.loadType(id));
		this.observe(this.unique, (key) => {
			if (key) {
				this.validation.setDataPath('$.' + dataPathPropertyName + `[?(@.key = '${key}')]`);
			}
		});
	}

	reset() {
		this.#data.setValue(undefined);
	}

	setVariantId(variantId: UmbVariantId | undefined) {
		this.#variantId.setValue(variantId);
	}
	getVariantId(): UmbVariantId {
		return this.#variantId.getValue() ?? UmbVariantId.CreateInvariant();
	}

	// TODO: rename to currentData:
	setData(data: UmbBlockDataModel | undefined) {
		this.#data.setValue(data);
		this.#getDataResolver();
	}

	getData() {
		return this.#data.getValue();
	}

	getUnique() {
		return this.getData()?.key;
	}

	getEntityType() {
		return 'element';
	}

	getContentTypeId() {
		return this.getData()?.contentTypeKey;
	}

	#createPropertyVariantId(property: UmbPropertyTypeModel, variantId: UmbVariantId) {
		return UmbVariantId.Create({
			culture: property.variesByCulture ? variantId.culture : null,
			segment: property.variesBySegment ? variantId.segment : null,
		});
	}

	// We will implement propertyAlias in the future, when implementing Varying Blocks. [NL]
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	async propertyVariantId(propertyAlias: string) {
		return mergeObservables(
			[await this.structure.propertyStructureByAlias(propertyAlias), this.variantId],
			([property, variantId]) =>
				property && variantId ? this.#createPropertyVariantId(property, variantId) : undefined,
		);
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias - Property Alias to observe the value of.
	 * @param {UmbVariantId | undefined} variantId - Optional variantId to filter by.
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>} - Promise which resolves to an Observable
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<PropertyValueType = unknown>(
		propertyAlias: string,
	): Promise<Observable<PropertyValueType | undefined> | undefined> {
		await this.#getDataPromise;

		return mergeObservables(
			[
				this.#data.asObservablePart((data) => data?.values?.filter((x) => x?.alias === propertyAlias)),
				await this.propertyVariantId(propertyAlias),
			],
			([propertyValues, propertyVariantId]) => {
				if (!propertyValues || !propertyVariantId) return;

				return propertyValues.find((x) => propertyVariantId.compare(x))?.value as PropertyValueType;
			},
		);
	}

	/**
	 * Get the current value of the property with the given alias and variantId.
	 * @param alias
	 * @param variantId
	 * @returns The value or undefined if not set or found.
	 */
	async getPropertyValue<ReturnType = unknown>(alias: string) {
		await this.#getDataPromise;
		const managerVariantId = this.#variantId.getValue();
		if (!managerVariantId) return;
		const property = await this.structure.getPropertyStructureByAlias(alias);
		if (!property) return;
		const variantId = this.#createPropertyVariantId(property, managerVariantId);
		const currentData = this.getData();
		if (!currentData) return;
		const newDataSet = currentData.values?.find((x) => x.alias === alias && (variantId ? variantId.compare(x) : true));
		return newDataSet?.value as ReturnType;
	}

	/**
	 * @function setPropertyValue
	 * @param {string} alias
	 * @param {unknown} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @param {UmbVariantId | undefined} variantId - Optional variantId to filter by.
	 * @returns {Promise<void>}
	 * @description Set the value of this property.
	 */
	async setPropertyValue<ValueType = unknown>(alias: string, value: ValueType) {
		this.initiatePropertyValueChange();
		await this.#getDataPromise;
		const managerVariantId = this.#variantId.getValue();
		if (!managerVariantId) return;
		const property = await this.structure.getPropertyStructureByAlias(alias);
		if (!property) return;
		const variantId = this.#createPropertyVariantId(property, managerVariantId);

		const entry = { ...variantId.toObject(), alias, value } as UmbBlockDataValueModel<ValueType>;
		const currentData = this.getData();
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values ?? [],
				entry,
				(x) => x.alias === alias && variantId.compare(x),
			);
			this.#data.update({ values });
		}
		this.finishPropertyValueChange();
	}

	#updateLock = 0;
	initiatePropertyValueChange() {
		this.#updateLock++;
		this.#data.mute();
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
			this.#data.unmute();
		}
	}

	public createPropertyDatasetContext(host: UmbControllerHost) {
		return new UmbBlockElementPropertyDatasetContext(host, this);
	}

	public setup(host: UmbClassInterface) {
		this.createPropertyDatasetContext(host);

		// Provide Validation Context for this view:
		this.validation.provideAt(host);
	}

	public override destroy(): void {
		this.#data.destroy();
		this.structure.destroy();
		super.destroy();
	}
}

export default UmbBlockElementManager;
