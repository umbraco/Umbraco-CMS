import type { UmbPropertyValueData } from '../types/property-value-data.type.js';
import { UMB_PROPERTY_DATASET_CONTEXT } from './property-dataset-context.token.js';
import type { UmbPropertyDatasetContext } from './property-dataset-context.interface.js';
import type { UmbNameablePropertyDatasetContext } from './nameable-property-dataset-context.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbArrayState, UmbBooleanState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A base property dataset context implementation.
 * @class UmbPropertyDatasetContextBase
 * @augments {UmbContextBase}
 */
export class UmbPropertyDatasetContextBase
	extends UmbContextBase
	implements UmbPropertyDatasetContext, UmbNameablePropertyDatasetContext
{
	#name = new UmbStringState(undefined);
	name = this.#name.asObservable();

	#properties = new UmbArrayState<UmbPropertyValueData>([], (x) => x.alias);
	public readonly properties = this.#properties.asObservable();
	/**
	 * @deprecated - use `properties` instead.
	 */
	readonly values = this.properties;

	private _entityType!: string;
	private _unique!: string;

	#readOnly = new UmbBooleanState(false);
	public readOnly = this.#readOnly.asObservable();

	getEntityType() {
		return this._entityType;
	}
	getUnique() {
		return this._unique;
	}
	getName() {
		return this.#name.getValue();
	}
	setName(name: string | undefined) {
		this.#name.setValue(name);
	}
	getVariantId() {
		return UmbVariantId.CreateInvariant();
	}
	// variant id for a specific property?

	constructor(host: UmbControllerHost) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, UMB_PROPERTY_DATASET_CONTEXT);
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias - the alias to observe
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>} - an Observable for the value of this property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#properties.asObservablePart((values) => {
			const valueObj = values.find((x) => x.alias === propertyAlias);
			return valueObj ? (valueObj.value as ReturnType) : undefined;
		});
	}

	/**
	 * @function setPropertyValue
	 * @param {string} alias - The alias to set this value for
	 * @param {PromiseLike<unknown>} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @description Set the value of this property.
	 */
	setPropertyValue(alias: string, value: unknown) {
		this.#properties.appendOne({ alias, value });
	}

	/**
	 * @deprecated Use `getProperties`
	 * @returns {Array<UmbPropertyValueData>} - Array of properties as objects with alias and value properties.
	 */
	getValues() {
		return this.#properties.getValue();
	}
	/**
	 * @param {Array<UmbPropertyValueData>} properties - Properties array with alias and value properties.
	 * @deprecated Use `setProperties`
	 */
	setValues(properties: Array<UmbPropertyValueData>) {
		this.#properties.setValue(properties);
	}

	/**
	 * @returns {Array<UmbPropertyValueData>} - Array of properties as objects with alias and value properties.
	 */
	async getProperties() {
		return this.#properties.getValue();
	}
	/**
	 * @param {Array<UmbPropertyValueData>} properties - Properties array with alias and value properties.
	 */
	setProperties(properties: Array<UmbPropertyValueData>) {
		this.#properties.setValue(properties);
	}

	/**
	 * Gets the read-only state of the current variant culture.
	 * @returns {*}  {boolean}
	 */
	getReadOnly(): boolean {
		return this.#readOnly.getValue();
	}
}
