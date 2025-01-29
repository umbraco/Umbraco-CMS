import type { UmbPropertyEditorConfigProperty, UmbPropertyEditorConfig } from '../index.js';
import type { DataTypePropertyPresentationModel } from '@umbraco-cms/backoffice/external/backend-api';

/**
 * Extends Array to add utility functions for accessing data type properties
 * by alias, returning either the value or the complete DataTypePropertyPresentationModel object
 */
export class UmbPropertyEditorConfigCollection extends Array<UmbPropertyEditorConfigProperty> {
	constructor(args: UmbPropertyEditorConfig) {
		super(...args);
	}
	static override get [Symbol.species](): ArrayConstructor {
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

	/**
	 * Convert the underlying array to an object where
	 * the property value is keyed by its alias
	 * eg
	 * `[
	 *   { 'alias': 'myProperty', 'value': 27 },
	 *   { 'alias': 'anotherProperty', 'value': 'eleven' },
	 * ]`
	 * is returned as
	 * `{
	 *   myProperty: 27,
	 * 	 anotherProperty: 'eleven',
	 * }`
	 */
	toObject(): Record<string, unknown> {
		return Object.fromEntries(this.map((x) => [x.alias, x.value]));
	}

	equal(other: UmbPropertyEditorConfigCollection | undefined): boolean {
		if (this.length !== other?.length) {
			return false;
		}

		return this.every((x) => {
			const otherProperty = x.alias ? other.getByAlias(x.alias!) : undefined;
			return otherProperty && x.value === otherProperty.value;
		});
	}
}
