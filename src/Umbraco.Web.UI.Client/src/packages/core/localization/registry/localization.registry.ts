import type { ManifestLocalization } from '../extensions/localization.extension.js';
import {
	type UmbLocalizationSetBase,
	type UmbLocalizationDictionary,
	type UmbLocalizationFlatDictionary,
	UMB_DEFAULT_LOCALIZATION_CULTURE,
} from '@umbraco-cms/backoffice/localization-api';
import { umbLocalizationManager } from '@umbraco-cms/backoffice/localization-api';
import type { UmbBackofficeExtensionRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { combineLatest } from '@umbraco-cms/backoffice/external/rxjs';
import { hasDefaultExport, loadManifestPlainJs } from '@umbraco-cms/backoffice/extension-api';

/**
 *
 * @param innerDictionary
 * @param dictionaryName
 * @param dictionary
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

	#loadedExtAliases: Array<string> = [];

	/**
	 * Get the current registered translations.
	 * @returns {Map<string, UmbLocalizationSetBase>} Returns the registered translations
	 */
	get localizations() {
		return umbLocalizationManager.localizations;
	}

	constructor(extensionRegistry: UmbBackofficeExtensionRegistry) {
		combineLatest([this.currentLanguage, extensionRegistry.byType('localization')]).subscribe(
			async ([currentLanguage, extensions]) => {
				const locale = new Intl.Locale(currentLanguage);
				const currentLanguageExtensions = extensions.filter(
					(ext) =>
						ext.meta.culture.toLowerCase() === locale.baseName.toLowerCase() ||
						ext.meta.culture.toLowerCase() === locale.language.toLowerCase(),
				);

				// If there are no extensions for the current language, return early
				if (!currentLanguageExtensions.length) return;

				// Register the new translations only if they have not been registered before
				const diff = currentLanguageExtensions.filter((ext) => !this.#loadedExtAliases.includes(ext.alias));

				// Load all localizations
				const translations = await Promise.all(currentLanguageExtensions.map(this.#loadExtension));

				// If there are no translations, return early
				if (!translations.length) return;

				if (diff.length) {
					const filteredTranslations = translations.filter((t) =>
						diff.some((ext) => ext.meta.culture.toLowerCase() === t.$code),
					);
					umbLocalizationManager.registerManyLocalizations(filteredTranslations);
				}

				// Set the document language
				const newLang = locale.baseName.toLowerCase();
				if (document.documentElement.lang.toLowerCase() !== newLang) {
					document.documentElement.lang = newLang;
				}

				// Set the document direction to the direction of the primary language
				const newDir = translations[0].$dir ?? 'ltr';
				if (document.documentElement.dir !== newDir) {
					document.documentElement.dir = newDir;
				}
			},
		);
	}

	#loadExtension = async (extension: ManifestLocalization) => {
		this.#loadedExtAliases.push(extension.alias);

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
			...innerDictionary,
		} satisfies UmbLocalizationSetBase;
	};

	/**
	 * Load a language from the extension registry.
	 * @param {string} locale The locale to load.
	 */
	loadLanguage(locale: string) {
		this.#currentLanguage.setValue(locale.toLowerCase());
	}
}

export const umbLocalizationRegistry = new UmbLocalizationRegistry(umbExtensionsRegistry);
