import type { ManifestPlainJs } from '@umbraco-cms/backoffice/extension-api';
import type { UmbLocalizationDictionary } from '@umbraco-cms/backoffice/localization-api';

export interface ManifestLocalization extends ManifestPlainJs<{ default: UmbLocalizationDictionary }> {
	type: 'localization';
	meta: MetaLocalization;
}

export interface MetaLocalization {
	/**
	 * @summary The culture of the translations.
	 * @description
	 * The culture is a combination of a language and a country. The language is represented by an ISO 639-1 code and the country is represented by an ISO 3166-1 alpha-2 code.
	 * The language and country are separated by a dash.
	 * The value is used to describe the language of the translations according to the extension system
	 * and it will be set as the `lang` attribute on the `<html>` element.
	 * @see https://en.wikipedia.org/wiki/Language_localisation#Language_tags_and_codes
	 * @example ["en-us", "en-gb", "da-dk"]
	 */
	culture: string;

	/**
	 * @summary The direction of the localizations (left-to-right or right-to-left).
	 * @description
	 * The value is used to describe the direction of the translations according to the extension system
	 * and it will be set as the `dir` attribute on the `<html>` element. It defaults to `ltr`.
	 * @see https://en.wikipedia.org/wiki/Right-to-left
	 * @example ["ltr"]
	 * @default "ltr"
	 */
	direction?: 'ltr' | 'rtl';

	/**
	 * The localizations.
	 * @example
	 * {
	 *   "general": {
	 *     "cancel": "Cancel",
	 *     "close": "Close"
	 *   }
	 * }
	 */
	localizations?: UmbLocalizationDictionary;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbLocalizationExtension: ManifestLocalization;
	}
}
