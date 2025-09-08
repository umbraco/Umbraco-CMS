import { UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT } from './property-type-based-property.context-token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';

export class UmbPropertyTypeBasedPropertyContext extends UmbContextBase {
	#unique = new UmbStringState(undefined);
	unique = this.#unique.asObservable();

	#dataType = new UmbObjectState<UmbPropertyTypeModel['dataType'] | undefined>(undefined);
	dataType = this.#dataType.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_PROPERTY_TYPE_BASED_PROPERTY_CONTEXT);
	}

	/**
	 * Sets the unique identifier of the Property Type
	 * @param unique - The unique identifier of the Property Type
	 */
	setUnique(unique: string | undefined) {
		this.#unique.setValue(unique);
	}

	/**
	 * Gets the unique identifier of the Property Type
	 * @returns {string | undefined} The unique identifier of the Property Type
	 */
	getUnique(): string | undefined {
		return this.#unique.getValue();
	}

	setDataType(dataType: UmbPropertyTypeModel['dataType'] | undefined) {
		this.#dataType.setValue(dataType);
	}
}

/**
 * @deprecated Use `UmbPropertyTypeBasedPropertyContext` instead.
 * This will be removed in v.18
 */
export { UmbPropertyTypeBasedPropertyContext as UmbContentPropertyContext };
