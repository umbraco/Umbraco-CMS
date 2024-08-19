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
	extends UmbContextBase<typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE>
	implements UmbPropertyDatasetContext, UmbNameablePropertyDatasetContext
{
	#name = new UmbStringState(undefined);
	name = this.#name.asObservable();

	#values = new UmbArrayState<UmbPropertyValueData>([], (x) => x.alias);
	public readonly values = this.#values.asObservable();
	private _entityType!: string;
	private _unique!: string;

	#currentVariantCultureIsReadOnly = new UmbBooleanState(false);
	public currentVariantCultureIsReadOnly = this.#currentVariantCultureIsReadOnly.asObservable();

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
	 * @param {string} propertyAlias
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>}
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#values.asObservablePart((values) => {
			const valueObj = values.find((x) => x.alias === propertyAlias);
			return valueObj ? (valueObj.value as ReturnType) : undefined;
		});
	}

	/**
	 * @function setPropertyValue
	 * @param {string} alias
	 * @param {PromiseLike<unknown>} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @returns {Promise<void>}
	 * @description Set the value of this property.
	 */
	setPropertyValue(alias: string, value: unknown) {
		this.#values.appendOne({ alias, value });
	}

	getValues() {
		return this.#values.getValue();
	}
	setValues(map: Array<UmbPropertyValueData>) {
		this.#values.setValue(map);
	}

	/**
	 * Gets the read-only state of the current variant culture.
	 * @return {*}  {boolean}
	 * @memberof UmbBlockGridInlinePropertyDatasetContext
	 */
	getCurrentVariantCultureIsReadOnly(): boolean {
		return this.#currentVariantCultureIsReadOnly.getValue();
	}
}
