import type { ManifestLocalization } from '../extensions/localization.extension.js';
import {
	catchError,
	distinctUntilChanged,
	filter,
	from,
	map,
	of,
	switchMap,
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

export class UmbLocalizationRegistry {
	#currentLanguage = new UmbStringState(
		document.documentElement.lang !== '' ? document.documentElement.lang : UMB_DEFAULT_LOCALIZATION_CULTURE,
	);
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
				// Switch to the extensions registry to get the current language and the extensions for that language
				// Note: This also cancels the previous subscription if the language changes
				switchMap((currentLanguage) => {
					return extensionRegistry.byType('localization').pipe(
						// Filter the extensions to only those that match the current language
						map((extensions) => {
							locale = new Intl.Locale(currentLanguage);
							return extensions.filter(
								(ext) =>
									ext.meta.culture.toLowerCase() === locale!.baseName.toLowerCase() ||
									ext.meta.culture.toLowerCase() === locale!.language.toLowerCase(),
							);
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

		// Always register the fallback language (en) to ensure there is always at least one language available
		this.loadLanguage(UMB_DEFAULT_LOCALIZATION_CULTURE);
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
		// Set the document language
		const newLang = locale.baseName.toLowerCase();
		if (document.documentElement.lang.toLowerCase() !== newLang) {
			document.documentElement.lang = newLang;
		}

		// We need to find the direction of the new language, so we look for the best match
		// If the new language is not found, we default to 'ltr'
		const reverseTranslations = translations.slice().reverse();

		// Look for a direct match first
		const directMatch = reverseTranslations.find((t) => t.$code.toLowerCase() === newLang);
		if (directMatch) {
			document.documentElement.dir = directMatch.$dir;
			return;
		}

		// If no direct match, look for a match with the language code only
		const langOnlyDirectMatch = reverseTranslations.find(
			(t) => t.$code.toLowerCase() === locale.language.toLowerCase(),
		);
		if (langOnlyDirectMatch) {
			document.documentElement.dir = langOnlyDirectMatch.$dir;
			return;
		}

		// If no match is found, default to 'ltr'
		if (document.documentElement.dir !== 'ltr') {
			document.documentElement.dir = 'ltr';
		}
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
	 * @param {string} locale The locale to load.
	 */
	loadLanguage(locale: string) {
		const canonicalLocale = Intl.getCanonicalLocales(locale)[0];
		this.#currentLanguage.setValue(canonicalLocale);
	}

	destroy() {
		this.#subscription.unsubscribe();
	}
}

export const umbLocalizationRegistry = new UmbLocalizationRegistry(umbExtensionsRegistry);
