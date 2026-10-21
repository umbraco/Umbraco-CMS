import type { UmbDocumentDetailModel } from '@umbraco-cms/backoffice/document';

export interface ExampleMediaPickerItem {
	mediaKey: string;
}

export interface ExampleDocumentPickerItem {
	unique: string;
	name?: string;
}

export interface ExampleRichTextValue {
	markup: string;
}

/**
 * Reads the key of the first item of a media picker value.
 * @param {unknown} value - The media picker value.
 * @returns {string | undefined} The media key, if any.
 */
export function getMediaKey(value: unknown): string | undefined {
	return (value as Array<ExampleMediaPickerItem> | undefined)?.[0]?.mediaKey;
}

/**
 * Reads the unique of the first item of a document picker value.
 * @param {unknown} value - The document picker value.
 * @returns {string | undefined} The document unique, if any.
 */
export function getDocumentUnique(value: unknown): string | undefined {
	return (value as Array<ExampleDocumentPickerItem> | undefined)?.[0]?.unique;
}

/**
 * @template T * Reads a property value of a document, preferring the given culture and falling back to an invariant value.
 * @param {UmbDocumentDetailModel | undefined} document - The document.
 * @param {string} alias - The property alias.
 * @param {string | undefined} culture - The culture to prefer.
 * @returns {T | undefined} The value, if any.
 */
export function getDocumentValue<T>(
	document: UmbDocumentDetailModel | undefined,
	alias: string,
	culture: string | undefined,
): T | undefined {
	const values = document?.values.filter((value) => value.alias === alias) ?? [];
	const match = values.find((value) => value.culture === culture) ?? values.find((value) => !value.culture);
	return (match ?? values[0])?.value as T | undefined;
}

/**
 * Reads the name of a document, preferring the given culture.
 * @param {UmbDocumentDetailModel | undefined} document - The document.
 * @param {string | undefined} culture - The culture to prefer.
 * @returns {string | undefined} The name, if any.
 */
export function getDocumentName(document: UmbDocumentDetailModel | undefined, culture: string | undefined) {
	const variants = document?.variants ?? [];
	return (variants.find((variant) => variant.culture === culture) ?? variants[0])?.name;
}
