import type { ManifestLocalization } from '../extensions/localization.extension.js';
import {
	catchError,
	distinctUntilChanged,
	filter,
	from,
	map,
	of,
	switchMap,
	tap,
} from '@umbraco-cms/backoffice/external/rxjs';
import { hasDefaultExport, loadManifestPlainJs } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { umbLocalizationManager, UMB_DEFAULT_LOCALIZATION_CULTURE } from '@umbraco-cms/backoffice/localization-api';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type {
	UmbLocalizationSetBase,
	UmbLocalizationDictionary,
	UmbLocalizationFlatDictionary,
} from '@umbraco-cms/backoffice/localization-api';
import type { Subscription } from '@umbraco-cms/backoffice/external/rxjs';

/**
 * Adds or updates a dictionary in the inner dictionary.
 * @param {UmbLocalizationFlatDictionary} innerDictionary The inner dictionary to add or update the dictionary in.
 * @param {string} dictionaryName The name of the dictionary to add or update.
 * @param {UmbLocalizationDictionary['value']} dictionary The dictionary to add or update.
 */
function addOrUpdateDictionary(
	innerDictionary: UmbLocalizationFlatDictionary,
	dictionaryName: string,
	dictionary: UmbLocalizationDictionary['value'],
) {
	for (const [key, value] of Object.entries(dictionary)) {
		innerDictionary[`${dictionaryName}_${key}`] = value;
	}
}

/**
 * Returns the canonical form of the given locale, or the default culture if the input is empty or
 * cannot be canonicalised. `Intl.getCanonicalLocales` throws RangeError for malformed input so we
 * intentionally swallow it here — a misconfigured DefaultUILanguage should fall back, not crash.
 * @param {string} locale - the locale to canonicalise.
 * @returns {string} the canonical locale, or the default culture if invalid.
 */
function toCanonicalLocale(locale: string): string {
	if (!locale) return UMB_DEFAULT_LOCALIZATION_CULTURE;
	try {
		return Intl.getCanonicalLocales(locale)[0] ?? UMB_DEFAULT_LOCALIZATION_CULTURE;
	} catch {
		return UMB_DEFAULT_LOCALIZATION_CULTURE;
	}
}

/**
 * Returns the lowercase BCP-47 base name (`language[-script][-region]`) of a locale tag, e.g.
 * `'en-US'` → `'en-us'`, `'zh-Hant-TW'` → `'zh-hant-tw'`. The input is expected to already be
 * a canonical locale (use {@link toCanonicalLocale} first if it might not be).
 * @param {string} locale - the locale to extract the base name from.
 * @returns {string} the lowercase base name of the locale.
 */
function baseLocaleOf(locale: string): string {
	return new Intl.Locale(locale).baseName.toLowerCase();
}

export class UmbLocalizationRegistry {
	#currentLanguage = new UmbStringState(toCanonicalLocale(document.documentElement.lang));
	readonly currentLanguage = this.#currentLanguage.asObservable();

	/**
	 * Get the current registered translations.
	 * @returns {Map<string, UmbLocalizationSetBase>} Returns the registered translations
	 */
	get localizations(): Map<string, UmbLocalizationSetBase> {
		return umbLocalizationManager.localizations;
	}

	#subscription: Subscription;

	constructor(extensionRegistry: UmbBackofficeExtensionRegistry) {
		// Store the locale in a variable to use when setting the document language and direction
		let locale: Intl.Locale | undefined = undefined;

		this.#subscription = this.currentLanguage
			.pipe(
				// Ensure the current language is not empty
				filter((currentLanguage) => !!currentLanguage),
				// Use distinctUntilChanged to avoid unnecessary re-renders when the language hasn't changed
				distinctUntilChanged(),
				// Mirror the active language onto the document and the manager synchronously, so a
				// fresh element rendering between now and the async translation load picks up the
				// requested language. Direction is set further down once we know which dictionary
				// won; consumers are notified there too.
				tap((currentLanguage) => {
					const newLang = baseLocaleOf(currentLanguage);
					if (document.documentElement.lang.toLowerCase() !== newLang) {
						document.documentElement.lang = newLang;
					}
					umbLocalizationManager.setActiveLanguage(newLang, umbLocalizationManager.documentDirection);
				}),
				// Switch to the extensions registry to get the current language and the extensions for that language
				// Note: This also cancels the previous subscription if the language changes
				switchMap((currentLanguage) => {
					return extensionRegistry.byType('localization').pipe(
						// Filter the extensions to those matching the current language; we also always
						// include the default culture so the manager has it available as a key-level
						// fallback when the active language is missing a translation.
						map((extensions) => {
							locale = new Intl.Locale(currentLanguage);
							const baseName = locale.baseName.toLowerCase();
							const language = locale.language.toLowerCase();
							return extensions.filter((ext) => {
								const culture = ext.meta.culture.toLowerCase();
								return culture === UMB_DEFAULT_LOCALIZATION_CULTURE || culture === baseName || culture === language;
							});
						}),
					);
				}),
				// Ensure we only process extensions that are registered
				filter((extensions) => extensions.length > 0),
				// Ensure we only process extensions that have not been loaded before
				distinctUntilChanged((prev, curr) => {
					const prevAliases = prev.map((ext) => ext.alias).sort();
					const currAliases = curr.map((ext) => ext.alias).sort();
					return this.#arraysEqual(prevAliases, currAliases);
				}),
				// With switchMap, if a new language is selected before the previous translations finish loading,
				// the previous promise is canceled (unsubscribed), and only the latest one is processed.
				// This prevents race conditions and stale state.
				switchMap((extensions) =>
					from(
						(async () => {
							// Load all localizations
							const translations = await Promise.all(extensions.map(this.#loadExtension));

							// If there are no translations, return early
							if (!translations.length) return;

							// Sort translations by their original extension weight (highest-to-lowest)
							// This ensures that the translations with the lowest weight override the others
							translations.sort((a, b) => b.$weight - a.$weight);

							// Load the translations into the localization manager
							umbLocalizationManager.registerManyLocalizations(translations);

							// Set the browser language and direction based on the translations
							this.#setBrowserLanguage(locale!, translations);
						})(),
					),
				),
				// Catch any errors that occur while loading the translations
				// This is important to ensure that the observable does not error out and stop the subscription
				catchError((error) => {
					console.error('Error loading translations:', error);
					return of([]);
				}),
			)
			// Subscribe to the observable to trigger the loading of translations
			.subscribe();
	}

	#loadExtension = async (extension: ManifestLocalization) => {
		const innerDictionary: UmbLocalizationFlatDictionary = {};

		// If extension contains a dictionary, add it to the inner dictionary.
		if (extension.meta.localizations) {
			for (const [dictionaryName, dictionary] of Object.entries(extension.meta.localizations)) {
				addOrUpdateDictionary(innerDictionary, dictionaryName, dictionary);
			}
		}

		// If extension contains a js file, load it and add the default dictionary to the inner dictionary.
		if (extension.js) {
			const loadedExtension = await loadManifestPlainJs(extension.js);

			if (loadedExtension && hasDefaultExport<UmbLocalizationDictionary>(loadedExtension)) {
				for (const [dictionaryName, dictionary] of Object.entries(loadedExtension.default)) {
					addOrUpdateDictionary(innerDictionary, dictionaryName, dictionary);
				}
			}
		}

		// Notify subscribers that the inner dictionary has changed.
		return {
			$code: extension.meta.culture.toLowerCase(),
			$dir: extension.meta.direction ?? 'ltr',
			$weight: extension.weight ?? 100,
			...innerDictionary,
		} satisfies UmbLocalizationSetBase & { $weight: number };
	};

	#setBrowserLanguage(locale: Intl.Locale, translations: UmbLocalizationSetBase[]) {
		const newLang = locale.baseName.toLowerCase();

		// Regional match wins, then language-only match, then default to LTR.
		const reverseTranslations = translations.slice().reverse();
		const bestMatch =
			reverseTranslations.find((t) => t.$code.toLowerCase() === newLang) ??
			reverseTranslations.find((t) => t.$code.toLowerCase() === locale.language.toLowerCase());
		const direction: 'ltr' | 'rtl' = bestMatch?.$dir ?? 'ltr';

		if (document.documentElement.dir !== direction) {
			document.documentElement.dir = direction;
		}
		umbLocalizationManager.setActiveLanguage(newLang, direction);
		umbLocalizationManager.notifyLanguageChanged();
	}

	#arraysEqual(a: string[], b: string[]) {
		if (a.length !== b.length) return false;
		for (let i = 0; i < a.length; i++) {
			if (a[i] !== b[i]) return false;
		}
		return true;
	}

	/**
	 * Load a language from the extension registry.
	 * @param {string} locale The locale to load. Invalid or empty input falls back to the default culture.
	 */
	loadLanguage(locale: string) {
		this.#currentLanguage.setValue(toCanonicalLocale(locale));
	}

	destroy() {
		this.#subscription.unsubscribe();
	}
}

export const umbLocalizationRegistry = new UmbLocalizationRegistry(umbExtensionsRegistry);
