import { TranslationSet, registerTranslation } from '../manager.js';
import { hasDefaultExport, loadExtension } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbBackofficeExtensionRegistry,
	UmbTranslationEntry,
	UmbTranslationsDictionary,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { Subject, combineLatest } from '@umbraco-cms/backoffice/external/rxjs';

export type UmbTranslationsFlatDictionary = Record<string, UmbTranslationEntry>;

export class UmbTranslationRegistry {
	#registry;
	#currentLanguage = new Subject<string>();

	constructor(extensionRegistry: UmbBackofficeExtensionRegistry) {
		this.#registry = extensionRegistry;

		combineLatest([this.#currentLanguage, this.#registry.extensionsOfType('translations')]).subscribe(
			async ([userCulture, extensions]) => {
				await Promise.all(
					extensions
						.filter((x) => x.meta.culture.toLowerCase() === userCulture)
						.map(async (extension) => {
							const innerDictionary: UmbTranslationsFlatDictionary = {};

							// If extension contains a dictionary, add it to the inner dictionary.
							if (extension.meta.translations) {
								for (const [dictionaryName, dictionary] of Object.entries(extension.meta.translations)) {
									this.#addOrUpdateDictionary(innerDictionary, dictionaryName, dictionary);
								}
							}

							// If extension contains a js file, load it and add the default dictionary to the inner dictionary.
							const loadedExtension = await loadExtension(extension);

							if (loadedExtension && hasDefaultExport<UmbTranslationsDictionary>(loadedExtension)) {
								for (const [dictionaryName, dictionary] of Object.entries(loadedExtension.default)) {
									this.#addOrUpdateDictionary(innerDictionary, dictionaryName, dictionary);
								}
							}

							// Notify subscribers that the inner dictionary has changed.
							const translation: TranslationSet = {
								$code: userCulture,
								$dir: extension.meta.direction ?? 'ltr',
								...innerDictionary,
							};
							registerTranslation(translation);

							// Set the document language and direction.
							document.documentElement.lang = translation.$code;
							document.documentElement.dir = translation.$dir;
						})
				);
			}
		);
	}

	/**
	 * Load a language from the extension registry.
	 * @param locale The locale to load.
	 */
	loadLanguage(locale: Intl.BCP47LanguageTag | Intl.Locale) {
		locale = new Intl.Locale(locale);
		this.#currentLanguage.next(`${locale.language}-${locale.region}`.toLowerCase());
	}

	#addOrUpdateDictionary(
		innerDictionary: UmbTranslationsFlatDictionary,
		dictionaryName: string,
		dictionary: UmbTranslationsDictionary['value']
	) {
		for (const [key, value] of Object.entries(dictionary)) {
			innerDictionary[`${dictionaryName}_${key}`] = value;
		}
	}
}

export const umbTranslationRegistry = new UmbTranslationRegistry(umbExtensionsRegistry);
