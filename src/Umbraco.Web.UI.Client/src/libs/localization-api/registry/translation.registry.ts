import { TranslationSet, registerTranslation } from '../manager.js';
import { hasDefaultExport, loadExtension } from '@umbraco-cms/backoffice/extension-api';
import {
	UmbBackofficeExtensionRegistry,
	UmbTranslationEntry,
	UmbTranslationsDictionary,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { Subject, combineLatest, map, distinctUntilChanged, Observable } from '@umbraco-cms/backoffice/external/rxjs';

export type UmbTranslationsFlatDictionary = Record<string, UmbTranslationEntry>;

export class UmbTranslationRegistry {
	#currentLanguage = new Subject<string>();
	#currentLanguageUnique: Observable<string> = this.#currentLanguage.pipe(
		map((x) => x.toLowerCase()),
		distinctUntilChanged()
	);

	constructor(extensionRegistry: UmbBackofficeExtensionRegistry) {
		combineLatest([this.#currentLanguageUnique, extensionRegistry.extensionsOfType('translations')]).subscribe(
			async ([userCulture, extensions]) => {
				const locale = new Intl.Locale(userCulture);
				const translations = await Promise.all(
					extensions
						.filter(
							(x) =>
								x.meta.culture.toLowerCase() === locale.baseName.toLowerCase() ||
								x.meta.culture.toLowerCase() === locale.language.toLowerCase()
						)
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
							return {
								$code: userCulture,
								$dir: extension.meta.direction ?? 'ltr',
								...innerDictionary,
							} satisfies TranslationSet;
						})
				);

				if (translations.length) {
					registerTranslation(...translations);

					// Set the document language
					document.documentElement.lang = locale.baseName;

					// Set the document direction to the direction of the primary language
					document.documentElement.dir = translations[0].$dir ?? 'ltr';
				}
			}
		);
	}

	/**
	 * Load a language from the extension registry.
	 * @param locale The locale to load.
	 */
	loadLanguage(locale: string) {
		this.#currentLanguage.next(locale);
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
