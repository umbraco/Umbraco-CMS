import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * Extends Array to add utility functions for accessing data type properties
 * by alias, returning either the value or the complete DataTypePropertyPresentationModel object
 */
export class UmbDataTypePropertyCollection extends Array<DataTypePropertyPresentationModel> {
	constructor(args: Array<DataTypePropertyPresentationModel> = []) {
		super(...args);
	}
	static get [Symbol.species](): ArrayConstructor {
		return Array;
	}
	
	getValueByAlias<T>(alias: string): T | undefined {
		const property = this.getByAlias(alias);

		if (property?.value === undefined || property?.value === null) {
			return;
		}

		return property.value as T;
	}

	getByAlias(alias: string): DataTypePropertyPresentationModel | undefined {
		return this.find((x) => x.alias === alias);
	}
}
