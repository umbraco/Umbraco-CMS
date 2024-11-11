export interface UmbValidationMessageTranslator {
	/**
	 *
	 * @param path - The path to translate
	 * @returns {false | undefined | string} - Returns false if the path is not handled by this translator, undefined if the path is invalid, or the translated path as a string.
	 */
	translate(path: string): false | undefined | string;
}
