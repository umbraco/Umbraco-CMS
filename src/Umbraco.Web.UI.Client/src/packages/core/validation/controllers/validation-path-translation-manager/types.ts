import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export type * from './property-validation-path-translator.extension.js';

export interface UmbPropertyValidationPathTranslator extends UmbApi {
	/**
	 * Clones a property value.
	 * @param value The value to clone.
	 * @returns A promise that resolves with the clonal(cloned value).
	 */
	translate: UmbPropertyValidationPathTranslatorMethod;
}

/**
 * Clones a property value.
 * @param value The value to clone.
 * @returns A promise that resolves with the translated path.
 */
export type UmbPropertyValidationPathTranslatorMethod = <PropertyValueType>(
	paths: Array<string>,
	propertyData: PropertyValueType,
) => PromiseLike<Array<string>>;

export interface UmbValidationPathTranslator extends UmbApi {
	/**
	 * Clones a property value.
	 * @param value The value to clone.
	 * @returns A promise that resolves with the clonal(cloned value).
	 */
	translate: UmbPropertyValidationPathTranslatorMethod;
}

/**
 * Clones a property value.
 * @param value The value to clone.
 * @returns A promise that resolves with the translated path.
 */
export type UmbValidationPathTranslatorMethod = (
	paths: Array<string>,
	propertyData: unknown,
) => PromiseLike<Array<string>>;
