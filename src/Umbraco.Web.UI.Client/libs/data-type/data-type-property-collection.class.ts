import { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/backend-api';

/**
 * Extends Array to add utility functions for accessing data type properties
 * by alias, returning either the value or the complete DataTypePropertyPresentationModel object
 */
export class UmbDataTypePropertyCollection extends Array<DataTypePropertyPresentationModel> {
	constructor(args: Array<DataTypePropertyPresentationModel> = []) {
		super(...args);
	}

	private _getByAlias(alias: string) {
		return this.find((x) => x.alias === alias);
	}

	getValueByAlias<T>(alias: string): T | undefined {
		const property = this._getByAlias(alias);

		if (property?.value === undefined || property?.value === null) {
			return;
		}

		return property.value as T;
	}

	getByAlias(alias: string): DataTypePropertyPresentationModel | undefined {
		const property = this._getByAlias(alias);
		return property;
	}
}
