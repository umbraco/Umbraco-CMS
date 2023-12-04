import type { UmbPropertyValueData } from '../types/property-value-data.type.js';
import { type UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_VARIANT_CONTEXT, UmbVariantContext } from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

/**
 * A basic variant context, holds a name and a a set of properties.
 * This is the base type for all variant contexts.
 */
export class UmbBasicVariantContext
	extends UmbContextBase<typeof UMB_VARIANT_CONTEXT.TYPE>
	implements UmbVariantContext
{
	#name = new UmbStringState('');
	name = this.#name.asObservable();

	#values = new UmbArrayState<UmbPropertyValueData<unknown>>([], (x) => x.alias);
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
	setName(name: string) {
		this.#name.next(name);
	}
	getVariantId() {
		return UmbVariantId.CreateInvariant();
	}
	// variant id for a specific property?

	constructor(host: UmbControllerHost) {
		// The controller alias, is a very generic name cause we want only one of these for this controller host.
		super(host, UMB_VARIANT_CONTEXT);
	}

	/**
	 * TODO: Write proper JSDocs here.
	 */
	propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
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
}
