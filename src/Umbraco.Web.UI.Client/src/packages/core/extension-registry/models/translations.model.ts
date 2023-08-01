import type { ManifestDefaultExport } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTranslations extends ManifestDefaultExport<Record<string, Record<string, unknown>>> {
	type: 'translations';
	meta: MetaTranslations;
}

export interface MetaTranslations {
	/**
	 * @summary The culture of the translations.
	 * @description
	 * The culture is a combination of a language and a country. The language is represented by an ISO 639-1 code and the country is represented by an ISO 3166-1 alpha-2 code.
	 * The language and country are separated by a dash.
	 * The value is used to describe the language of the translations according to the extension system
	 * and it will be set as the `lang` attribute on the `<html>` element.
	 * @see https://en.wikipedia.org/wiki/Language_localisation#Language_tags_and_codes
	 * @examples ["en-us", "en-gb", "da-dk"]
	 */
	culture: string;

	/**
	 * @summary The direction of the translations (left-to-right or right-to-left).
	 * @description
	 * The value is used to describe the direction of the translations according to the extension system
	 * and it will be set as the `dir` attribute on the `<html>` element. It defaults to `ltr`.
	 * @see https://en.wikipedia.org/wiki/Right-to-left
	 * @examples ["ltr"]
	 * @default "ltr"
	 */
	direction?: 'ltr' | 'rtl';

	/**
	 * The translations.
	 * @example
	 * {
	 *   "general": {
	 *     "cancel": "Cancel",
	 *     "close": "Close"
	 *   }
	 * }
	 */
	translations?: Record<string, Record<string, string>>;
}
