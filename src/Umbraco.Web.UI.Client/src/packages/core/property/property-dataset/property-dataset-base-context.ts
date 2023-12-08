import type { UmbPropertyValueData } from '../../workspace/types/property-value-data.type.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import {
	UMB_PROPERTY_DATASET_CONTEXT,
	type UmbNameablePropertyDatasetContext,
	type UmbPropertyDatasetContext,
} from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A base property dataset context implementation.
 * @class UmbPropertyDatasetBaseContext
 * @extends {UmbContextBase}
 */
export class UmbPropertyDatasetBaseContext
	extends UmbContextBase<typeof UMB_PROPERTY_DATASET_CONTEXT.TYPE>
	implements UmbPropertyDatasetContext, UmbNameablePropertyDatasetContext
{
	#name = new UmbStringState(undefined);
	name = this.#name.asObservable();

	#values = new UmbArrayState<UmbPropertyValueData>([], (x) => x.alias);
	public readonly values = this.#values.asObservable();
	private _entityType!: string;
	private _unique!: string;

	getType() {
		return this._entityType;
	}
	getUnique() {
		return this._unique;
	}
	getName() {
		return this.#name.getValue();
	}
	setName(name: string | undefined) {
		this.#name.next(name);
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
	 * TODO: Write proper JSDocs here.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#values.asObservablePart((values) => {
			const valueObj = values.find((x) => x.alias === propertyAlias);
			return valueObj ? (valueObj.value as ReturnType) : undefined;
		});
	}

	/**
	 * TODO: Write proper JSDocs here.
	 */
	setPropertyValue(alias: string, value: unknown) {
		this.#values.appendOne({ alias, value });
	}

	getValues() {
		return this.#values.getValue();
	}
	setValues(map: Array<UmbPropertyValueData>) {
		this.#values.next(map);
	}
}
