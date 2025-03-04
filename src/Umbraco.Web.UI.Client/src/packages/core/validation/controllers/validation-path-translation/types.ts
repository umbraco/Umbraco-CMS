import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPropertyValueDataPotentiallyWithEditorAlias } from '@umbraco-cms/backoffice/property';

export type * from './property-validation-path-translator.extension.js';

export interface UmbPropertyValidationPathTranslator<PropertyValueType> extends UmbApi {
	/**
	 * Clones a property value.
	 * @param value The value to clone.
	 * @returns A promise that resolves with the clonal(cloned value).
	 */
	translate: UmbPropertyValidationPathTranslatorMethod<PropertyValueType>;
}

/**
 * Clones a property value.
 * @param value The value to clone.
 * @returns A promise that resolves with the translated path.
 */
export type UmbPropertyValidationPathTranslatorMethod<PropertyValueType> = (
	paths: Array<string>,
	propertyData: UmbPropertyValueDataPotentiallyWithEditorAlias<PropertyValueType>,
) => PromiseLike<Array<string>>;

export interface UmbValidationPathTranslator<T> extends UmbApi {
	/**
	 * Clones a property value.
	 * @param value The value to clone.
	 * @returns A promise that resolves with the clonal(cloned value).
	 */
	translate: UmbValidationPathTranslatorMethod<T>;
}

/**
 * Clones a property value.
 * @param value The value to clone.
 * @returns A promise that resolves with the translated path.
 */
export type UmbValidationPathTranslatorMethod<T> = (
	paths: Array<string>,
	propertyData: T,
) => PromiseLike<Array<string>>;
